using NSmartProxy.Client;
using NSmartProxy.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace NSmartProxy.Client
{
    public class ClientGroupEventArgs : EventArgs
    {
        public IEnumerable<TcpClient> NewClients;
        public ClientIdAppId App;
    }

    public class ServerConnnectionManager
    {
        private int MAX_CONNECT_SIZE = 6;
        private int ClientID = 0;
        private ServerConnnectionManager()
        {
            Console.WriteLine("ServerConnnectionManager initialized.");
            //获取服务端配置信息
            //测试 ，暂时设置为3
            //int length = Apps.Length


            //arrangedAppid = configBytes[]

            ///
            /// var tsk = Task.Run(PollingToProvider);
            /// tsk.Wait();
            ///   if (tsk.Exception != null) { Console.WriteLine(tsk.Exception.Message); };
        }

        /// <summary>
        /// 初始化配置，返回服务端返回的配置
        /// </summary>
        /// <returns></returns>
        public ClientModel InitConfig()
        {
            ClientModel clientModel = ReadConfigFromProvider();
            //要求服务端分配资源并获取服务端配置，待完善

            this.ClientID = clientModel.ClientId;
            //分配appid给不同的Client
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
            return clientModel;
        }

        /// <summary>
        /// 从服务端读取配置
        /// </summary>
        /// <returns></returns>
        private ClientModel ReadConfigFromProvider()
        {
            var config = NSmartProxy.Client.Router.ClientConfig;
            Console.WriteLine("Reading Config From Provider..");
            TcpClient configClient = new TcpClient();
            configClient.Connect(config.ProviderAddress, config.ProviderConfigPort);
            var configStream = configClient.GetStream();

            var requestBytes = new ClientNewAppRequest
            {
                ClientId = 0,
                ClientCount = config.Clients.Count(obj => obj.AppId == 0) //appid为0的则是未分配的
            }.ToBytes();
            configStream.Write(new ClientNewAppRequest
            {
                ClientId = 0,
                ClientCount = config.Clients.Count(obj => obj.AppId == 0) //appid为0的则是未分配的
            }.ToBytes(), 0, requestBytes.Length);
            byte[] serverConfig = new byte[256];
            int readBytesCount = configStream.Read(serverConfig, 0, serverConfig.Length);
            return ClientModel.GetFromBytes(serverConfig, readBytesCount);
        }

        public event EventHandler ClientGroupConnected;
        /// <summary>
        /// 将所有的app循环连接服务端
        /// </summary>
        /// <returns></returns>
        public async Task PollingToProvider()
        {
            var config = NSmartProxy.Client.Router.ClientConfig;
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
                                client.Connect(config.ProviderAddress, config.ProviderPort);
                                //连完了马上发送端口信息过去，方便服务端分配
                                client.GetStream().Write(requestBytes, 0, requestBytes.Length);
                                Console.WriteLine("ClientID:" + ClientID.ToString()
                                    + "AppId:" + appid.ToString() + " 已连接");
                                app.TcpClientGroup.Add(client);
                                clientList.Add(client);
                            }
                            ClientGroupConnected(this, new ClientGroupEventArgs()
                            {
                                NewClients = clientList,
                                App = new ClientIdAppId
                                {
                                    ClientId = ClientID,
                                    AppId = appid
                                }
                            });
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
        public Dictionary<int, ClientAppWorker> ServiceClientListCollection;// = new Dictionary<int, List<TcpClient>>();
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
