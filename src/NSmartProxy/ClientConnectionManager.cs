using NSmartProxy.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NSmartProxy.Authorize;
using NSmartProxy.Database;
using NSmartProxy.Infrastructure;
using NSmartProxy.Shared;
using static NSmartProxy.Server;

namespace NSmartProxy
{

    /// <summary>
    /// 反向连接处理类
    /// </summary>
    public class ClientConnectionManager
    {
        /// <summary>
        /// 当app增加时触发
        /// </summary>
       // public event EventHandler<AppChangedEventArgs> AppTcpClientMapReverseConnected = delegate { };
        public event EventHandler<AppChangedEventArgs> AppTcpClientMapConfigApplied = delegate { };

        private NSPServerContext ServerContext;


        private ClientConnectionManager()
        {
            Server.Logger.Debug("ClientManager initialized");
            // Task.Run(ListenServiceClient);
        }

        private readonly object _lockObject = new Object();
        private readonly object _lockObject2 = new Object();
        private readonly Random _rand = new Random();
        public async Task ListenServiceClient(IDbOperator dbOp)
        {
            //侦听，并且构造连接池
            Server.Logger.Debug("Listening client on port " + ServerContext.ServerConfig.ReversePort + "...");
            TcpListener listener = new TcpListener(IPAddress.Any, ServerContext.ServerConfig.ReversePort);
            listener.Start(1000);
            while (true)
            {
                SecurityTcpClient incomeClient = await listener.AcceptSecureTcpClientAsync(dbOp);
                Server.Logger.Debug("已建立一个空连接" +
                    incomeClient.Client.Client.LocalEndPoint.ToString() + "-" +
                    incomeClient.Client.Client.RemoteEndPoint.ToString());
                incomeClient.Client.SetKeepAlive(out _);
                _ = ProcessReverseRequest(incomeClient);
            }

        }

        public static ClientConnectionManager GetInstance()
        {
            return Instance;
        }

        public ClientConnectionManager SetServerContext(NSPServerContext serverContext)
        {
            ServerContext = serverContext;
            return this;
        }

        /// <summary>
        /// 处理反向连接请求（服务端）
        /// </summary>
        /// <param name="incomeClient"></param>
        /// <returns></returns>
        private async Task ProcessReverseRequest(SecurityTcpClient incomeClient)
        {
            var iClient = incomeClient.Client;
            try
            {
                var result = await incomeClient.AuthorizeAsync();
                if (result == null) return;//主动关闭

                if (!result.IsSuccess)
                {
                    Server.Logger.Debug("SecurityTcpClient校验失败：" + incomeClient.ErrorMessage);
                    await iClient.GetStream().WriteAsync(new byte[] { (byte)result.ResultState });
                    iClient.Close();//如果校验失败则发送一个字节的直接关闭连接
                }
                else
                {
                    Server.Logger.Debug("SecurityTcpClient校验成功！");
                    await iClient.GetStream().WriteAsync(new byte[] { (byte)result.ResultState });
                }

                //读取头四个字节
                byte[] bytes = new byte[4];
                if (await iClient.GetStream().ReadAsync(bytes, 0, bytes.Length, Global.DefaultConnectTimeout) < 1)
                {
                    Server.Logger.Debug("服务端read出错，关闭连接");
                    incomeClient.Client.Close();
                    return;
                }


                var clientIdAppId = GetAppFromBytes(bytes);
                Server.Logger.Debug("已获取到消息ClientID:" + clientIdAppId.ClientID
                                                      + "AppID:" + clientIdAppId.AppID
                );
                //分配
                lock (_lockObject)
                {
                    ServerContext.Clients[clientIdAppId.ClientID].GetApp(clientIdAppId.AppID)
                        .PushInComeClient(iClient);
                }
                //var arg = new AppChangedEventArgs();
                //arg.App = clientIdAppId;
                //AppTcpClientMapReverseConnected(this, arg);
            }
            catch (Exception e)
            {
                Logger.Debug(e);
            }

        }


        private static readonly ClientConnectionManager Instance = new Lazy<ClientConnectionManager>(() => new ClientConnectionManager()).Value;

        public TcpClient GetClientForUdp(int consumerPort, string host = null)
        {
            NSPApp nspApp = null;
            nspApp = ServerContext.UDPPortAppMap[consumerPort].ActivateApp;

            TcpClient client = nspApp.TcpClientBlocks.Peek();
            if (client == null)
            {
                throw new TimeoutException($"UDP获取{consumerPort}失败");
            }
            return client;
        }

        /// <summary>
        /// 从当前app的连接池取出一个socket
        /// </summary>
        /// <param name="consumerPort"></param>
        /// <param name="host"></param>
        /// <param name="onlySeek">仅仅获取这个client 而不消费它（开启新连接）</param>
        /// <returns></returns>
        public async Task<TcpClient> GetClientForTcp(int consumerPort, string host = null)
        {
            NSPApp nspApp = null;
            if (host == null || string.IsNullOrEmpty(host.Trim())) //host为空则随便匹配一个
            { nspApp = ServerContext.PortAppMap[consumerPort].ActivateApp; }
            else
            {
                if (!ServerContext.PortAppMap[consumerPort].TryGetValue(host, out nspApp))
                {
                    //throw new KeyNotFoundException($"无法找到{consumerPort}的host:{host}");
                    Server.Logger.Debug($"无法找到{consumerPort}的host:{host}");
                    nspApp = ServerContext.PortAppMap[consumerPort].ActivateApp;
                }
            }
            if (nspApp == null) throw new KeyNotFoundException($"无法找到{consumerPort}下的任何一个客户端app");

            //TODO ***需要处理服务端长时间不来请求的情况（无法建立隧道）
            //TODO 2这里的弹出比较麻烦了
            TcpClient client = await nspApp.PopClientAsync();
            if (client == null)
            {
                throw new TimeoutException($"弹出{consumerPort}超时");
            }

            //TODO 2 反向链接还写在这里？？
            //ServerContext.PortAppMap[consumerPort].ActivateApp.ReverseClients.Add(client);
            nspApp.ReverseClients.Add(client);
            return client;
        }

        /// <summary>
        /// 分配端口方法
        /// arrange ConfigId from top 4 bytes which received from client.
        /// response:
        ///   2          1       1       1           1        ...N
        ///  clientid    appid   port    appid2      port2
        /// request:
        ///   2          2
        ///  clientid    count
        ///  methodType  value = 0
        /// </summary>
        public byte[] ArrangeConfigIds(byte[] appRequestBytes, byte[] consumerPortBytes, int highPriorityClientId)
        {
            // byte[] arrangedBytes = new byte[256];
            ClientModel clientModel = new ClientModel();
            int clientId;
            //apprequestbytes里本身有clientId，但是如果传了highPriorityClientId，那就用这个clientId
            if (highPriorityClientId != 0)
            {
                clientId = highPriorityClientId;
            }
            else
            {
                clientId = StringUtil.DoubleBytesToInt(appRequestBytes[0], appRequestBytes[1]);
            }

            //2.分配clientid //TODO 这一段可能不会再用到了
            int appCount = (int)appRequestBytes[2];

            if (clientId == 0)
            {
                lock (_lockObject)
                {
                    byte[] tempClientIdBytes = new byte[2];
                    //分配clientid
                    for (int i = 0; i < 10000; i++)
                    {
                        _rand.NextBytes(tempClientIdBytes);
                        int tempClientId = StringUtil.DoubleBytesToInt(tempClientIdBytes);
                        if (!ServerContext.Clients.ContainsKey(tempClientId))
                        {

                            clientModel.ClientId = tempClientId;
                            clientId = tempClientId;

                            break;
                        }
                    }
                }
            }
            else
            {
                clientModel.ClientId = clientId;
            }

            //注册客户端
            ServerContext.Clients.RegisterNewClient(clientModel.ClientId);
            //port proto option(iscompress)  host         description   
            //2    1     1                   1024         96         
            int oneEndpointLength = 2 + 1 + 1 + 1024 + 96;
            lock (_lockObject2)
            {
                //注册app
                clientModel.AppList = new List<App>(appCount);
                for (int i = 0; i < appCount; i++)
                {
                    int offset = oneEndpointLength * i;
                    int startPort = StringUtil.DoubleBytesToInt(consumerPortBytes[offset], consumerPortBytes[offset + 1]);
                    int arrangedAppid = ServerContext.Clients[clientId].RegisterNewApp();
                    Protocol protocol = (Protocol)consumerPortBytes[offset + 2];
                    bool isCompress = consumerPortBytes[offset + 3] == 1 ? true : false;
                    string host = Encoding.ASCII.GetString(consumerPortBytes, offset + 4, 1024).TrimEnd('\0');
                    string description = Encoding.UTF8.GetString(consumerPortBytes, offset + 4 + 1024, 96).TrimEnd('\0');
                    //查找port的起始端口如果未指定，则设置为20000
                    if (startPort == 0) startPort = Global.StartArrangedPort;
                    int port = 0;
                    //如果端口是指定的并且是绑定的，不加任何检测
                    bool hasListened = false;
                    if (IsBoundedByUser(clientId, startPort))
                    {
                        port = startPort;
                    }
                    else
                    {
                        if (protocol == Protocol.TCP)
                        {
                            int relocatedPort = NetworkUtil.FindOneAvailableTCPPort(startPort);
                            port = relocatedPort; 
                        }
                        else if (protocol == Protocol.UDP)
                        {
                            int relocatedPort = NetworkUtil.FindOneAvailableUDPPort(startPort);
                            port = relocatedPort;
                        }
                        else if (protocol == Protocol.HTTP)
                        {//因为端口复用，允许公用端口
                            int relocatedPort = NetworkUtil.FindOneAvailableTCPPort(startPort);
                            //兼容http侦听端口公用
                            if (port != relocatedPort)
                            {
                                //http协议下如果portappmap已经有值，说明已经发起过侦听，接下来不必再侦听
                                if (ServerContext.PortAppMap.ContainsKey(startPort))
                                {
                                    port = startPort;
                                    hasListened = true;
                                }
                                else
                                {
                                    port = relocatedPort;
                                }
                            }
                        }

                    }
                    NSPApp app = ServerContext.Clients[clientId].AppMap[arrangedAppid];
                    app.ClientId = clientId;
                    app.AppId = arrangedAppid;
                    app.ConsumePort = port;
                    app.AppProtocol = protocol;
                    app.Host = host;
                    app.Description = description;
                    app.Tunnels = new List<TcpTunnel>();
                    app.ReverseClients = new List<TcpClient>();
                    app.IsCompress = isCompress;

                    if (protocol == Protocol.UDP)
                    {
                        if (!ServerContext.UDPPortAppMap.ContainsKey(port))
                        {
                            ServerContext.UDPPortAppMap[port] = new NSPAppGroup();
                        }
                        ServerContext.UDPPortAppMap[port][host] = app;
                    }
                    else
                    {
                        //TODO 设置app的host和protocoltype(TCP Only)
                        if (!ServerContext.PortAppMap.ContainsKey(port))
                        {
                            ServerContext.PortAppMap[port] = new NSPAppGroup();
                        }
                        ServerContext.PortAppMap[port][host] = app;
                    }

                    clientModel.AppList.Add(new App
                    {
                        AppId = arrangedAppid,
                        Port = port
                    });

                    Logger.Info(port);
                    //配置时触发
                    if (!hasListened)
                    {
                        AppTcpClientMapConfigApplied(this, new AppChangedEventArgs() { App = app });//触发listener侦听
                    }
                }
                Logger.Debug(" <=端口已分配。");
            }
            return clientModel.ToBytes();
        }

        private bool IsBoundedByUser(int clientId, int port)
        {
            var boundHash = ServerContext.ServerConfig.BoundConfig.UserPortBounds;
            if (boundHash.TryGetValue(clientId.ToString(), out UserPortBound userBound))
            {
                return userBound.Bound.Contains(port);
            }
            return false;
        }

        private ClientIDAppID GetAppFromBytes(byte[] bytes)
        {
            return new ClientIDAppID()
            {
                ClientID = (bytes[0] << 8) + bytes[1],
                AppID = bytes[2]
            };
        }
    }
}
