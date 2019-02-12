using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSmartProxy
{
    public struct TcpTunnel
    {
        public int ClientID;
        public int AppID;
        public int Port;
    }
    public class ClientConnectionManager
    {
        private ClientConnectionManager()
        {
            Console.WriteLine("ClientManager initialized");
            Task.Run(ListenServiceClient);
        }

        /// <summary>
        /// 客户端，appid，端口映射
        /// </summary>
        public Dictionary<int, List<TcpTunnel>> ServiceClientsDict = new Dictionary<int, List<TcpTunnel>>();
        //port->tcptunnels->tcpclients
        private Dictionary<TcpTunnel, Queue<TcpClient>> ServiceClientQueueCollection = new Dictionary<TcpTunnel, Queue<TcpClient>>();

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
                //根据不同的服务端appid安排不同的连接池
                AddClient(GetTcpTunnelFromBytes(bytes), incomeClient);
            }

        }

        //可能要改成字典
        //private Queue<TcpClient> ServiceClientQueue = new Queue<TcpClient>();
       
        private static ClientConnectionManager Instance = new Lazy<ClientConnectionManager>(() => new ClientConnectionManager()).Value;
        public void AddClient(TcpTunnel tcpTunnel, TcpClient client)
        {
            ServiceClientQueueCollection[tcpTunnel].Enqueue(client);
        }

        public static ClientConnectionManager GetInstance()
        {
            return Instance;
        }

        public TcpClient GetClient(int port)
        {
            ServiceClientsDict[port][0]
            return ServiceClientQueueCollection[tcpTunnel].Dequeue();
        }

        //arrange ConfigId from top 4 bytes which received from client.
        //response:
        //   2          1       1       1           1        ...N
        //  clientid    appid   port    appid2      port2
        public byte[] ArrageConfigIds(byte[] fourBytes)
        {
            byte[] arrangedBytes = new byte[256];
            int clientId = fourBytes[0] << 8 + fourBytes[1];
            int appCount = (int)fourBytes[2];
            if (clientId == 0)
            {
                lock (_lockObject)
                {
                    byte[] tempClientIdBytes = new byte[2];
                    for (int i = 0; i < 10000; i++)
                    {
                        _rand.NextBytes(tempClientIdBytes);
                        int tempClientId = tempClientIdBytes[0] << 8 + tempClientIdBytes[1];
                        if (!ServiceClientsDict.ContainsKey(tempClientId))
                        {
                            arrangedBytes[0] = tempClientIdBytes[0];
                            arrangedBytes[1] = tempClientIdBytes[1];
                            clientId = tempClientId;
                            ServiceClientsDict.Add(clientId, new List<TcpTunnel>());
                            break;
                        }
                    }
                }
            }
            else
            {
                arrangedBytes[0] = fourBytes[0];
                arrangedBytes[1] = fourBytes[1];
            }
            lock (_lockObject2)
            {
                int maxAppCount = ServiceClientsDict[clientId].Count;
                //增加请求的客户端
                int[] ports = NetworkUtil.FindAvailableTCPPorts(20000, appCount);
                for (int i = 0; i < appCount; i++)
                {
                    int arrangedAppid = maxAppCount + i;
                    if (arrangedAppid > 255) throw new Exception("Stack overflow.");
                    //获取可用端口，增加到tcpclient
                    ServiceClientsDict[clientId].Add(new TcpTunnel()
                    {
                        ClientID = clientId,
                        AppID = arrangedAppid,
                        Port = ports[i]
                    });
                    arrangedBytes[i + 2] = (byte)arrangedAppid;
                }
            }

            return arrangedBytes;
        }

        /// <summary>
        /// 解析客户端请求的tcp连接分类
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private TcpTunnel GetTcpTunnelFromBytes(byte[] bytes)
        {
            return new TcpTunnel()
            {
                ClientID = bytes[0] << 8 + bytes[1],
                AppID = bytes[2],
                Port = 0
            };
        }
    }
}
