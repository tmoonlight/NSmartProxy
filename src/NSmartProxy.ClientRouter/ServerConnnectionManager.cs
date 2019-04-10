using NSmartProxy.Client;
using NSmartProxy.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using NSmartProxy.Infrastructure;
using NSmartProxy.Shared;

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
        private int _clientID = 0;

        public List<TcpClient> ConnectedConnections;
        public Dictionary<int, ClientAppWorker> ServiceClientListCollection;  //key:appid value;ClientApp
        public Action ServerNoResponse = delegate { };

        public int ClientID
        {
            get => _clientID;
        }

        private ServerConnnectionManager()
        {
            ConnectedConnections = new List<TcpClient>();
            Router.Logger.Debug("ServerConnnectionManager initialized.");
        }
        /// <summary>
        /// 初始化配置，返回服务端返回的配置
        /// </summary>
        /// <returns></returns>
        public async Task<ClientModel> InitConfig()
        {
            ClientModel clientModel = await ReadConfigFromProvider();

            //要求服务端分配资源并获取服务端配置
            this._clientID = clientModel.ClientId;
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
            var delayDispose = Task.Delay(TimeSpan.FromSeconds(Global.DefaultConnectTimeout)).ContinueWith(_ => configClient.Dispose());
            var connectAsync = configClient.ConnectAsync(config.ProviderAddress, config.ProviderConfigPort);
            //超时则dispose掉
            var comletedTask = await Task.WhenAny(delayDispose, connectAsync);
            if (!connectAsync.IsCompleted)
            {
                throw new Exception("ReadConfigFromProvider连接超时");
            }

            var configStream = configClient.GetStream();

            //请求0 协议名
            byte requestByte0 = (byte)Protocol.ClientNewAppRequest;
            await configStream.WriteAndFlushAsync(new byte[] { requestByte0 }, 0, 1);

            //请求1 端口数
            var requestBytes = new ClientNewAppRequest
            {
                ClientId = 0,
                ClientCount = config.Clients.Count(obj => obj.AppId == 0) //appid为0的则是未分配的
            }.ToBytes();
            await configStream.WriteAndFlushAsync(requestBytes, 0, requestBytes.Length);

            //请求2 分配端口
            byte[] requestBytes2 = new byte[config.Clients.Count * 2];
            int i = 0;
            foreach (var client in config.Clients)
            {
                byte[] portBytes = StringUtil.IntTo2Bytes(client.ConsumerPort);
                requestBytes2[2 * i] = portBytes[0];
                requestBytes2[2 * i + 1] = portBytes[1];
                i++;
            }
            await configStream.WriteAndFlushAsync(requestBytes2, 0, requestBytes2.Length);

            //读端口配置
            byte[] serverConfig = new byte[256];
            int readBytesCount = await configStream.ReadAsync(serverConfig, 0, serverConfig.Length);
            if (readBytesCount == 0) Router.Logger.Debug("服务器状态异常，已断开连接");

            return ClientModel.GetFromBytes(serverConfig, readBytesCount);
        }

        /// <summary>
        /// clients Connected event.
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

            foreach (var kv in ServiceClientListCollection)
            {

                int appid = kv.Key;
                await ConnectAppToServer(appid);


            }
        }

        public async Task ConnectAppToServer(int appid)
        {
            var app = this.ServiceClientListCollection[appid];
            var config = NSmartProxy.Client.Router.ClientConfig;
            // ClientAppWorker app = kv.Value;
            byte[] requestBytes = StringUtil.ClientIDAppIdToBytes(ClientID, appid);
            var clientList = new List<TcpClient>();
            //补齐
            TcpClient client = new TcpClient();
            try
            {
                //1.连接服务端
                await client.ConnectAsync(config.ProviderAddress, config.ProviderPort);
                //2.发送clientid和appid信息，向服务端申请连接
                //连接到位后增加相关的元素并且触发客户端连接事件
                await client.GetStream().WriteAndFlushAsync(requestBytes, 0, requestBytes.Length);
                Router.Logger.Debug("ClientID:" + ClientID.ToString()
                                                + " AppId:" + appid.ToString() + " 已连接");
            }
            catch (Exception ex)
            {
                Router.Logger.Error("反向连接出错！:" + ex.Message, ex);

                //TODO 回收隧道

            }

            app.TcpClientGroup.Add(client);
            clientList.Add(client);
            //统一管理连接
            ConnectedConnections.AddRange(clientList);

            //事件循环1,这个方法必须放在最后
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

        public async Task StartHeartBeats(int interval, CancellationToken ct)
        {
            try
            {
                var config = NSmartProxy.Client.Router.ClientConfig;

                //TODO 客户端开启心跳
                while (!ct.IsCancellationRequested)
                {
                    TcpClient client;
                    try
                    {
                        client = await NetworkUtil.ConnectAndSend(config.ProviderAddress,
                            config.ProviderConfigPort, Protocol.Heartbeat, StringUtil.IntTo2Bytes(this.ClientID));
                    }
                    catch (Exception ex)
                    {
                        Router.Logger.Debug(ex);
                        ServerNoResponse();
                        break;
                    }

                    //1.发送心跳
                    using (client)
                    {
                        //2.接收ack 超时则重发
                        byte[] onebyte = new byte[1];
                        //Router.Logger.Debug("读ack");
                        var delayDispose =
                            Task.Delay(Global.DefaultWriteAckTimeout); //.ContinueWith(_ => client.Dispose());

                        var readBytes = client.GetStream().ReadAsync(onebyte, 0, 1);
                        //超时则dispose掉
                        var comletedTask = await Task.WhenAny(delayDispose, readBytes);

                        if (!readBytes.IsCompleted)
                        {
                            //TODO 连接超时，需要外部处理，暂时无法内部处理
                            Router.Logger.Error("服务端心跳连接超时", new Exception("服务端心跳连接超时"));
                            ServerNoResponse();
                            break;
                        }
                        else if (readBytes.Result == 0)
                        {
                            //TODO 连接已关闭
                            Router.Logger.Debug("服务端心跳连接已关闭");
                            ServerNoResponse();
                            break;
                        }

                        //Router.Logger.Debug("接收到ack");
                    }

                    await Task.Delay(interval, ct);
                }
            }
            catch (Exception ex)
            {
                Router.Logger.Error("fatal error: Heartbeat错误:" + ex.Message, ex);
                throw;
            }
            finally
            {
                Router.Logger.Debug("心跳连接终止。");
                await Task.Delay(1000);
                //TODO 重启
            }

        }


    }
}
