using NSmartProxy.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSmartProxy
{
    public class ClientGroupEventArgs : EventArgs
    {
        public IEnumerable<TcpClient> NewClients;
    }

    public class ServerConnnectionManager
    {
        private int MAX_CONNECT_SIZE = 10;
        private int ClientID = 0;
        private ServerConnnectionManager()
        {
            Console.WriteLine("ServerConnnectionManager initialized.");
            //获取服务端配置信息
            //测试 ，暂时设置为3
            //int length = Apps.Length
            byte[] configBytes = ReadConfigFromProvider();
            //※要求服务端分配资源并获取服务端配置※，待完善
            //Console.WriteLine(config[0] + "!!!!!!!!!");
            //唯一的clientid
            ClientID = configBytes[0] << 8 + configBytes[1];
            //分配appid
            ServiceClientListCollection = new Dictionary<int, List<TcpClient>>();
            for (int i = 2; i < configBytes.Length; i++)
            {
                if (configBytes[i] == 0)
                    break;
                ServiceClientListCollection.Add((int)configBytes[i], new List<TcpClient>());
            }
            //arrangedAppid = configBytes[]

            Task.Run(PollingToProvider);
        }

        /// <summary>
        /// 从服务端读取配置
        /// </summary>
        /// <returns></returns>
        private static byte[] ReadConfigFromProvider()
        {
            TcpClient configClient = new TcpClient();
            configClient.Connect(ClientRouter.PROVIDER_ADDRESS, ClientRouter.PROVIDER_CONFIG_SERVICE_PORT);
            var configStream = configClient.GetStream();
            byte[] fourBytes = new byte[4] { 0, 0, 3, 0 };
            configStream.Write(fourBytes);
            byte[] config = new byte[256];
            configStream.Read(config);
            return config;
        }

        public static event EventHandler ClientGroupConnected;
        private async Task PollingToProvider()
        {
            if (ClientID == 0) { Console.WriteLine("error:未连接客户端"); return; };
            int hungryNumber = MAX_CONNECT_SIZE / 2;
            byte[] clientBytes = StringUtil.IntToBytes(ClientID);
            //侦听，并且构造连接池
            //throw new NotImplementedException();
            //int currentClientCount = ServiceClientQueue.Count;
            List<Task> taskList = new List<Task>();
            foreach (var kv in ServiceClientListCollection)
            {
                int appid = kv.Key;
                byte[] requestBytes = StringUtil.Generate1stRequestBytes(ClientID,appid);
                taskList.Add(Task.Run(async () =>
                {
                    while (1 == 1)
                    {

                        int activeClientCount = 0;
                        foreach (TcpClient c in ServiceClientListCollection[appid])
                        {
                            if (c.Connected) activeClientCount++;
                        }
                        if (activeClientCount < hungryNumber)
                        {
                            Console.WriteLine("连接已接近饥饿值，扩充连接池");

                            var clientList = new List<TcpClient>();
                            //补齐
                            for (int i = activeClientCount; i < MAX_CONNECT_SIZE; i++)
                            {
                                TcpClient client = new TcpClient();
                                client.Connect(ClientRouter.PROVIDER_ADDRESS, ClientRouter.PROVIDER_ADDRESS_PORT);
                                //连完了马上发送端口信息过去，方便服务端分配
                                client.GetStream().Write(requestBytes);
                                ServiceClientListCollection[appid].Add(client);
                                clientList.Add(client);
                            }
                            ClientGroupConnected(this, new ClientGroupEventArgs() { NewClients = clientList });
                        }
                        await Task.Delay(2000);
                        //currentClientCount = ServiceClientQueue.Count;
                    }
                })
               );
            }
            Task resultTask = await Task.WhenAny(taskList);
            Console.WriteLine(resultTask.Exception?.ToString());
        }

        //可能要改成字典
        private Dictionary<int, List<TcpClient>> ServiceClientListCollection;// = new Dictionary<int, List<TcpClient>>();
        private static ServerConnnectionManager Instance = new Lazy<ServerConnnectionManager>(() => new ServerConnnectionManager()).Value;
        //Queue<TcpClient> IdleClientsQueue = new Queue<TcpClient>();
        public void AddClient(int appId, TcpClient client)
        {
            ServiceClientListCollection[appId].Add(client);
        }

        public static ServerConnnectionManager GetInstance()
        {
            return Instance;
        }

        public TcpClient RemoveClient(int appId, TcpClient client)
        {
            if (ServiceClientListCollection[appId].Remove(client))

                return client;
            else
            {
                throw new Exception("无此client");
            }
        }
    }
}
