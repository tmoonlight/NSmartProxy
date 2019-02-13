using NSmartProxy.Client;
using NSmartProxy.Data;
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
        public ClientIdAppId App;
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
            ClientModel clientModel = ReadConfigFromProvider();
            //要求服务端分配资源并获取服务端配置，待完善
            //Console.WriteLine(config[0] + "!!!!!!!!!");
            //唯一的clientid
            this.ClientID = clientModel.ClientId;
            //分配appid
            ServiceClientListCollection = new Dictionary<int, ClientAppWorker>();
            for (int i = 0; i < clientModel.AppList.Count; i++)
            {
                var app = clientModel.AppList[i];
                ServiceClientListCollection.Add(clientModel.AppList[i].AppId, new ClientAppWorker()
                {
                    AppId = app.AppId,
                    Port = app.Port,
                    TcpClientGroup = new List<TcpClient>(MAX_CONNECT_SIZE)
                });
            }
            //arrangedAppid = configBytes[]

            Task.Run(PollingToProvider);
        }

        /// <summary>
        /// 从服务端读取配置
        /// </summary>
        /// <returns></returns>
        private static ClientModel ReadConfigFromProvider()
        {
            TcpClient configClient = new TcpClient();
            configClient.Connect(ClientRouter.PROVIDER_ADDRESS, ClientRouter.PROVIDER_CONFIG_SERVICE_PORT);
            var configStream = configClient.GetStream();

            //byte[] fourBytes = new byte[4] { 0, 0, 3, 0 };
            configStream.Write(new ClientNewAppRequest { ClientId = 0, ClientCount = 3 }.ToBytes());
            byte[] config = new byte[256];
            int readBytesCount = configStream.Read(config);
            return ClientModel.GetFromBytes(config, readBytesCount);
        }

        public static event EventHandler ClientGroupConnected;
        private async Task PollingToProvider()
        {
            if (ClientID == 0) { Console.WriteLine("error:未连接客户端"); return; };
            int hungryNumber = MAX_CONNECT_SIZE / 2;
            byte[] clientBytes = StringUtil.IntTo2Bytes(ClientID);
            //侦听，并且构造连接池
            //throw new NotImplementedException();
            //int currentClientCount = ServiceClientQueue.Count;
            List<Task> taskList = new List<Task>();
            foreach (var kv in ServiceClientListCollection)
            {
                int appid = kv.Key;
                ClientAppWorker app = kv.Value;
                byte[] requestBytes = StringUtil.ClientIDAppIdToBytes(ClientID, appid);
                taskList.Add(Task.Run(async () =>
                {
                    while (1 == 1)
                    {

                        int activeClientCount = 0;
                        foreach (TcpClient c in app.TcpClientGroup)
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
                                app.TcpClientGroup.Add(client);
                                clientList.Add(client);
                            }
                            ClientGroupConnected(this, new ClientGroupEventArgs() {
                                NewClients = clientList,
                                App = new ClientIdAppId { ClientId = ClientID, AppId = appid
                             } });
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

        //key:appid value;ClientApp
        private Dictionary<int, ClientAppWorker> ServiceClientListCollection;// = new Dictionary<int, List<TcpClient>>();
        private static ServerConnnectionManager Instance = new Lazy<ServerConnnectionManager>(() => new ServerConnnectionManager()).Value;
        //Queue<TcpClient> IdleClientsQueue = new Queue<TcpClient>();
        //public void AddClient(int appId, TcpClient client)
        //{
        //    ServiceClientListCollection[appId].Add(client);
        //}

        public static ServerConnnectionManager GetInstance()
        {
            return Instance;
        }

        public TcpClient RemoveClient(int appId, TcpClient client)
        {
            if (ServiceClientListCollection[appId].TcpClientGroup.Remove(client))

                return client;
            else
            {
                throw new Exception("无此client");
            }
        }
    }
}
