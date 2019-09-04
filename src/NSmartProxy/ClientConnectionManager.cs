using NSmartProxy.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public event EventHandler<AppChangedEventArgs> AppTcpClientMapReverseConnected = delegate { };
        public event EventHandler<AppChangedEventArgs> AppTcpClientMapConfigConnected = delegate { };

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


        //public async Task<TcpClient> GetHTTPClient(int consumerPort)
        //{
        //    var clientId = ServerContext.PortAppMap[consumerPort].ClientId;
        //    var appId = ServerContext.PortAppMap[consumerPort].AppId;

        //    //TODO ***需要处理服务端长时间不来请求的情况（无法建立隧道）
        //    TcpClient client = await ServerContext.Clients[clientId].AppMap[appId].PopClientAsync();
        //    if (client == null) return null;
        //    ServerContext.PortAppMap[consumerPort].ReverseClients.Add(client);
        //    return client;
        //}

        public async Task<TcpClient> GetClient(int consumerPort)
        {
            var clientId = ServerContext.PortAppMap[consumerPort].ActivateApp.ClientId;
            var appId = ServerContext.PortAppMap[consumerPort].ActivateApp.AppId;

            //TODO ***需要处理服务端长时间不来请求的情况（无法建立隧道）
            TcpClient client = await ServerContext.Clients[clientId].AppMap[appId].PopClientAsync();
            if (client == null) return null;
            ServerContext.PortAppMap[consumerPort].ActivateApp.ReverseClients.Add(client);
            return client;
        }

        //通过客户端的id请求，分配好服务端端口和appid交给客户端
        //arrange ConfigId from top 4 bytes which received from client.
        //response:
        //   2          1       1       1           1        ...N
        //  clientid    appid   port    appid2      port2
        //request:
        //   2          2
        //  clientid    count
        //  methodType  value = 0
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
                        int tempClientId = (tempClientIdBytes[0] << 8) + tempClientIdBytes[1];
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
            int oneEndpointLength = 2 + 1 + 1024;
            lock (_lockObject2)
            {
                //注册app
                clientModel.AppList = new List<App>(appCount);
                for (int i = 0; i < appCount; i++)
                {
                    int startPort = StringUtil.DoubleBytesToInt(consumerPortBytes[oneEndpointLength * i], consumerPortBytes[oneEndpointLength * i + 1]);
                    int arrangedAppid = ServerContext.Clients[clientId].RegisterNewApp();
                    Protocol protocol = (Protocol)consumerPortBytes[3];
                    string host = Encoding.ASCII.GetString(consumerPortBytes, 3, 1024);
                    //查找port的起始端口如果未指定，则设置为20000
                    if (startPort == 0) startPort = Global.StartArrangedPort;
                    int port = 0;
                    //如果端口是指定的并且是绑定的，不加任何检测
                    if (IsBoundedByUser(clientId, startPort))
                    {
                        port = startPort;
                    }
                    else
                    {
                        port = NetworkUtil.FindOneAvailableTCPPort(startPort);
                    }
                    NSPApp app = ServerContext.Clients[clientId].AppMap[arrangedAppid];
                    app.ClientId = clientId;
                    app.AppId = arrangedAppid;
                    app.ConsumePort = port;
                    app.Tunnels = new List<TcpTunnel>();
                    app.ReverseClients = new List<TcpClient>();
                    //app.Host = host;
                    //TODO 设置app的host和protocoltype
                    if (protocol == Protocol.HTTP)
                    {
                        ServerContext.PortAppMap[port].Add(host, app);
                    }

                    ServerContext.PortAppMap[port].ActivateApp = app;

                    clientModel.AppList.Add(new App
                    {
                        AppId = arrangedAppid,
                        Port = port
                    });

                    Logger.Info(port);
                    //配置时触发
                    AppTcpClientMapConfigConnected(this, new AppChangedEventArgs() { App = app });
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
