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
        //public event EventHandler<AppChangedEventArgs> AppRemoved = delegate { };
        public NSPClientCollection Clients = new NSPClientCollection();

        //端口和app的映射关系
        public Dictionary<int, NSPApp> PortAppMap = new Dictionary<int, NSPApp>();

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
            Server.Logger.Debug("Listening client on port " + Server.ClientServicePort + "...");
            TcpListener listener = new TcpListener(IPAddress.Any, Server.ClientServicePort);
            listener.Start(1000);
            while (true)
            {
                SecurityTcpClient incomeClient = await listener.AcceptSecureTcpClientAsync(dbOp);
                Server.Logger.Debug("已建立一个空连接" +
                    incomeClient.Client.Client.LocalEndPoint.ToString() + "-" +
                    incomeClient.Client.Client.RemoteEndPoint.ToString());
                incomeClient.Client.SetKeepAlive(out _);
                ProcessReverseRequest(incomeClient);
            }

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
                await iClient.GetStream().ReadAsync(bytes);

                var clientIdAppId = GetAppFromBytes(bytes);
                Server.Logger.Debug("已获取到消息ClientID:" + clientIdAppId.ClientID
                                                      + "AppID:" + clientIdAppId.AppID
                );
                //分配
                lock (_lockObject)
                {
                    Clients[clientIdAppId.ClientID].GetApp(clientIdAppId.AppID)
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

        public static ClientConnectionManager GetInstance()
        {
            return Instance;
        }

        public async Task<TcpClient> GetClient(int consumerPort)
        {
            var clientID = PortAppMap[consumerPort].ClientId;
            var appID = PortAppMap[consumerPort].AppId;

            //TODO ***需要处理服务端长时间不来请求的情况（无法建立隧道）
            TcpClient client = await Clients[clientID].AppMap[appID].PopClientAsync();
            PortAppMap[consumerPort].ReverseClients.Add(client);
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
        public byte[] ArrageConfigIds(byte[] appRequestBytes, byte[] consumerPortBytes)
        {
            // byte[] arrangedBytes = new byte[256];
            ClientModel clientModel = new ClientModel();
            int clientId = (appRequestBytes[0] << 8) + appRequestBytes[1];
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
                        if (!Clients.ContainsKey(tempClientId))
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
            Clients.RegisterNewClient(clientModel.ClientId);
            lock (_lockObject2)
            {
                //注册app
                clientModel.AppList = new List<App>(appCount);
                for (int i = 0; i < appCount; i++)
                {
                    int startPort = StringUtil.DoubleBytesToInt(consumerPortBytes[2 * i], consumerPortBytes[2 * i + 1]);

                    int arrangedAppid = Clients[clientId].RegisterNewApp();
                    //查找port的起始端口如果未指定，则设置为20000
                    if (startPort == 0) startPort = 20000;
                    int port = NetworkUtil.FindOneAvailableTCPPort(startPort);
                    NSPApp app = Clients[clientId].AppMap[arrangedAppid];
                    app.ClientId = clientId;
                    app.AppId = arrangedAppid;
                    app.ConsumePort = port;
                    app.Tunnels = new List<TcpTunnel>();
                    app.ReverseClients = new List<TcpClient>();
                    PortAppMap[port] = app;

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
