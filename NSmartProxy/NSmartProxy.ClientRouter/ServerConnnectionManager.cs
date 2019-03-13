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
        private int MAX_CONNECT_SIZE = 6;//magic value,单个应用最大连接数,有些应用端支持多连接，需要调高此值，当该值较大时，此值会增加
        private int ClientID = 0;
        private ServerConnnectionManager()
        {
            Router.Logger.Debug("ServerConnnectionManager initialized.");
        }

        /// <summary>
        /// 初始化配置，返回服务端返回的配置
        /// </summary>
        /// <returns></returns>
        public async Task<ClientModel> InitConfig()
        {
            ClientModel clientModel = await ReadConfigFromProvider();
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
        private async Task<ClientModel> ReadConfigFromProvider()
        {
            //《c#并发编程经典实例》 9.3 超时后取消
            var config = NSmartProxy.Client.Router.ClientConfig;
            Router.Logger.Debug("Reading Config From Provider..");
            TcpClient configClient = new TcpClient();
            var delayDispose = Task.Delay(TimeSpan.FromSeconds(2)).ContinueWith(_ => configClient.Dispose());
            var connectAsync = configClient.ConnectAsync(config.ProviderAddress, config.ProviderConfigPort);
            //超时则dispose掉
            var comletedTask = await Task.WhenAny(delayDispose, connectAsync);
            if (!connectAsync.IsCompleted)
            {
                throw new Exception("连接超时");
            }

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
            if (readBytesCount == 0) Router.Logger.Debug("服务器状态异常，已断开连接");
            return ClientModel.GetFromBytes(serverConfig, readBytesCount);
        }

        /// <summary>
        /// client firstConnected event.
        /// </summary>
        public event EventHandler ClientGroupConnected;

        /// <summary>
        /// 将所有的app循环连接服务端
        /// </summary>
        /// <returns></returns>
        public async Task PollingToProvider()
        {
            var config = NSmartProxy.Client.Router.ClientConfig;
            if (ClientID == 0) { Router.Logger.Debug("error:未连接客户端"); return; };
            //int hungryNumber = MAX_CONNECT_SIZE / 2;
            byte[] clientBytes = StringUtil.IntTo2Bytes(ClientID);

            List<Task> taskList = new List<Task>();
            foreach (var kv in ServiceClientListCollection)
            {
                //优化，只连接一个，维持一个备用连接。
                int appid = kv.Key;
                TcpClient client =await ConnectAppToServer(appid);

                var clientList = new List<TcpClient>() { client };
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
        }

        public async Task<TcpClient> ConnectAppToServer(int appid)
        {
            var app = this.ServiceClientListCollection[appid];
            var config = NSmartProxy.Client.Router.ClientConfig;
            // ClientAppWorker app = kv.Value;
            byte[] requestBytes = StringUtil.ClientIDAppIdToBytes(ClientID, appid);
            var clientList = new List<TcpClient>();
            //补齐
            TcpClient client = new TcpClient();
            await client.ConnectAsync(config.ProviderAddress, config.ProviderPort);
            //连完了马上发送端口信息过去，方便服务端分配
            await  client.GetStream().WriteAsync(requestBytes, 0, requestBytes.Length);
            Router.Logger.Debug("ClientID:" + ClientID.ToString()
                                            + " AppId:" + appid.ToString() + " 已连接");
            app.TcpClientGroup.Add(client);
            //clientList.Add(client);
            return client;
        }

        //key:appid value;ClientApp
        public Dictionary<int, ClientAppWorker> ServiceClientListCollection;// = new Dictionary<int, List<TcpClient>>();
        //private static ServerConnnectionManager Instance = new Lazy<ServerConnnectionManager>(() => new ServerConnnectionManager()).Value;



        public static ServerConnnectionManager Create()
        {
            return new ServerConnnectionManager();
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
