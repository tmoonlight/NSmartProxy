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

        //端口和app的映射关系，需定时清理
        public Dictionary<int, AppModel> PortAppMap = new Dictionary<int, AppModel>();

        //app和代理客户端socket之间的映射关系
        public ConcurrentDictionary<ClientIDAppID, BufferBlock<TcpClient>> AppTcpClientMap = new ConcurrentDictionary<ClientIDAppID, BufferBlock<TcpClient>>();

        //已注册的clientID,和appid之间的关系,appid序号=元素下标序号+1
        public Dictionary<int, List<ClientIDAppID>> RegisteredClient = new Dictionary<int, List<ClientIDAppID>>();

        private ClientConnectionManager()
        {
            Server.Logger.Debug("ClientManager initialized");
            Task.Run(ListenServiceClient);
        }

        private object _lockObject = new Object();
        private object _lockObject2 = new Object();
        private Random _rand = new Random();
        private async Task ListenServiceClient()
        {
            //侦听，并且构造连接池
            Server.Logger.Debug("Listening client on port " + Server.ClientServicePort + "...");
            TcpListener listenter = new TcpListener(IPAddress.Any, Server.ClientServicePort);
            listenter.Start(1000);
            while (true)
            {
                TcpClient incomeClient = await listenter.AcceptTcpClientAsync();
                Server.Logger.Debug("已建立一个空连接");
                ProcessReverseRequest(incomeClient);
            }

        }

        /// <summary>
        /// 处理反向连接请求
        /// </summary>
        /// <param name="incomeClient"></param>
        /// <returns></returns>
        private async Task ProcessReverseRequest(TcpClient incomeClient)
        {
            try
            {
                //读取头四个字节
                byte[] bytes = new byte[4];
                await incomeClient.GetStream().ReadAsync(bytes);

                var clientIdAppId = GetAppFromBytes(bytes);
                Server.Logger.Debug("已获取到消息ClientID:" + clientIdAppId.ClientID.ToString()
                                                      + "AppID:" + clientIdAppId.AppID.ToString()
                );
                //分配
                lock (_lockObject)
                {
                    AppTcpClientMap.GetOrAdd(clientIdAppId, new BufferBlock<TcpClient>()).Post(incomeClient);
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


        private static ClientConnectionManager Instance = new Lazy<ClientConnectionManager>(() => new ClientConnectionManager()).Value;

        public static ClientConnectionManager GetInstance()
        {
            return Instance;
        }

        public async Task<TcpClient> GetClient(int consumerPort)
        {
            //从字典的list中取出tcpclient，并将其移除
            ClientIDAppID clientappid = PortAppMap[consumerPort].ClientIdAppId;
            
            TcpClient client = await AppTcpClientMap[clientappid].ReceiveAsync();
            PortAppMap[consumerPort].ReverseClients.Add(client);
            // AppTcpClientMap[clientappid].Remove(client);
            //AppRemoved(this, new AppChangedEventArgs { App = clientappid });
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
        public byte[] ArrageConfigIds(byte[] appRequestBytes)
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
                        if (!RegisteredClient.ContainsKey(tempClientId))
                        {

                            clientModel.ClientId = tempClientId;
                            clientId = tempClientId;
                            //注册客户端
                            RegisteredClient.Add(tempClientId, new List<ClientIDAppID>());
                            break;
                        }
                    }
                }
            }
            else
            {
                clientModel.ClientId = clientId;
            }
            lock (_lockObject2)
            {
                //循环获取appid，appid是元素下标+1
                int maxAppCount = RegisteredClient[clientId].Count;
                //增加请求的客户端
                int[] ports = NetworkUtil.FindAvailableTCPPorts(20000, appCount);
                foreach (var oneport in ports) Logger.Info(oneport + " ");
                Logger.Debug(" <=端口已分配。");
                clientModel.AppList = new List<App>(appCount);
                for (int i = 0; i < appCount; i++)
                {
                    int arrangedAppid = maxAppCount + i + 1;
                    if (arrangedAppid > 255) throw new Exception("Stack overflow.");
                    //获取可用端口，增加到tcpclient
                    RegisteredClient[clientId].Add(new ClientIDAppID
                    {
                        ClientID = clientId,
                        AppID = arrangedAppid
                    });
                    clientModel.AppList.Add(new App
                    {
                        AppId = arrangedAppid,
                        Port = ports[i]
                    });
                    var appClient = PortAppMap[ports[i]] = new AppModel()
                    {
                        ClientIdAppId = new ClientIDAppID()
                        {
                            ClientID = clientId,
                            AppID = arrangedAppid
                        },
                        Tunnels = new List<TcpTunnel>(),
                        ReverseClients =  new List<TcpClient>()
                    };
                    //配置时触发
                    AppTcpClientMapConfigConnected(this, new AppChangedEventArgs() { App = appClient.ClientIdAppId });
                }

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
