using NSmartProxy.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSmartProxy
{
    public struct ClientIDAppID
    {
        public int ClientID;
        public int AppID;
    }
    public class ClientConnectionManager
    {
        //端口和ClientIDAppID的映射关系
        public Dictionary<int, ClientIDAppID> PortAppMap;
        //app和代理客户端socket之间的映射关系
        public Dictionary<ClientIDAppID, List<TcpClient>> AppTcpClientMap = new Dictionary<ClientIDAppID, List<TcpClient>>();

        //已注册的clientID,和appid之间的关系,appid序号=元素下标序号+1
        public Dictionary<int, List<ClientIDAppID>> RegisteredClient = new Dictionary<int, List<ClientIDAppID>>();

        private ClientConnectionManager()
        {
            Console.WriteLine("ClientManager initialized");
            Task.Run(ListenServiceClient);
        }

        /// <summary>
        /// 客户端，appid，端口映射
        /// </summary>
        // public Dictionary<int, List<TcpTunnel>> ServiceClientsDict = new Dictionary<int, List<TcpTunnel>>();
        //port->tcptunnels->tcpclients
        // private Dictionary<TcpTunnel, Queue<TcpClient>> ServiceClientQueueCollection = new Dictionary<TcpTunnel, Queue<TcpClient>>();

        private object _lockObject = new Object();
        private object _lockObject2 = new Object();
        private Random _rand = new Random();
        private async Task ListenServiceClient()
        {
            //侦听，并且构造连接池
            TcpListener listenter = new TcpListener(IPAddress.Any, Server.CLIENT_SERVER_PORT);
            listenter.Start(1000);
            while (1 == 1)
            {
                TcpClient incomeClient = await listenter.AcceptTcpClientAsync();
                Console.WriteLine("已建立一个空连接");
                //立即侦听一次并且分配连接
                byte[] bytes = new byte[4];
                await incomeClient.GetStream().ReadAsync(bytes);
                var clientIdAppId = GetAppFromBytes(bytes);
                //根据不同的服务端appid安排不同的连接池
                AppTcpClientMap[clientIdAppId].Add(incomeClient);
                //AddClient(GetTcpTunnelFromBytes(bytes), incomeClient);
            }

        }

        //可能要改成字典
        //private Queue<TcpClient> ServiceClientQueue = new Queue<TcpClient>();

        private static ClientConnectionManager Instance = new Lazy<ClientConnectionManager>(() => new ClientConnectionManager()).Value;
        //public void AddClient(TcpTunnel tcpTunnel, TcpClient client)
        //{
        //    ServiceClientQueueCollection[tcpTunnel].Enqueue(client);
        //}

        public static ClientConnectionManager GetInstance()
        {
            return Instance;
        }

        public TcpClient GetClient(int consumerPort)
        {
            //从字典的list中取出tcpclient，并将其移除
            ClientIDAppID clientappid = PortAppMap[consumerPort];
            TcpClient client = AppTcpClientMap[clientappid][0];
            AppTcpClientMap[clientappid].Remove(client);
            return client;
            //ServiceClientsDict[port][0]
            //return ServiceClientQueueCollection[tcpTunnel].Dequeue();
        }

        //通过客户端的id请求，分配好服务端端口和appid交给客户端
        //arrange ConfigId from top 4 bytes which received from client.
        //response:
        //   2          1       1       1           1        ...N
        //  clientid    appid   port    appid2      port2
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
                    PortAppMap[ports[i]] = new ClientIDAppID
                    {
                        ClientID = clientId,
                        AppID = arrangedAppid
                    };


            }
            }
            return clientModel.ToBytes();
        }

        ///// <summary>
        ///// 解析客户端请求的tcp连接分类
        ///// </summary>
        ///// <param name="bytes"></param>
        ///// <returns></returns>
        //private TcpTunnel GetTcpTunnelFromBytes(byte[] bytes)
        //{
        //    return new TcpTunnel()
        //    {
        //        ClientID = (bytes[0] << 8) + bytes[1],
        //        AppID = bytes[2],
        //        Port = 0
        //    };
        //}

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
