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
using NSmartProxy.Authorize;
using System.IO;
using NSmartProxy.Client.Authorize;
using System.Collections.Concurrent;

namespace NSmartProxy.Client
{
    public class ClientGroupEventArgs : EventArgs
    {
        public IEnumerable<TcpClient> NewClients;
        public ClientIdAppId App;
    }

    public class ServerConnectionManager
    {
        //private int MAX_CONNECT_SIZE = 6;//magic value,单个应用最大连接数,有些应用端支持多连接，需要调高此值，当该值较大时，此值会增加
        private int _clientID = 0;

        public List<TcpClient> ConnectedConnections;
        public Dictionary<string, UdpClient> ConnectedUdpClients;//ip:port->udpclient
        public ServiceClientListCollection ServiceClientList;  //key:appid value;ClientApp
        public Action ServerNoResponse = delegate { };
        public NSPClientConfig ClientConfig;

        public int ClientID
        {
            get => _clientID;
        }

        public string CurrentToken { get; set; } = Global.NO_TOKEN_STRING;

        private ServerConnectionManager()
        {
            ConnectedConnections = new List<TcpClient>();
            ConnectedUdpClients = new Dictionary<string, UdpClient>();
            Router.Logger.Debug("ServerConnectionManager initialized.");
        }
        /// <summary>
        /// 初始化配置，返回服务端返回的配置
        /// </summary>
        /// <returns></returns>
        public async Task<ClientModel> InitConfig(NSPClientConfig config)
        {
            //TODO 7 需要重构
            ClientConfig = config;
            ClientModel clientModel = null;
            try
            {
                clientModel = await SendConfigRequest();
            }
            catch (Exception ex) //如果这里出错，则自动删除缓存
            {
                //TODO 2 判断服务端返回错误类型，如果是校验错误，则清空缓存

                throw ex;
            }

            //要求服务端分配资源并获取服务端配置
            this._clientID = clientModel.ClientId;
            //分配appid给不同的Client
            ServiceClientList = new ServiceClientListCollection();
            for (int i = 0; i < clientModel.AppList.Count; i++)
            {
                var app = clientModel.AppList[i];
                ServiceClientList.Add(clientModel.AppList[i].AppId, new ClientAppWorker()
                {
                    AppId = app.AppId,
                    Port = app.Port,
                    Client = new TcpClient()
                });
            }
            return clientModel;
        }

        /// <summary>
        /// 从服务端读取配置，N问一答模式
        /// </summary>
        /// <returns></returns>
        private async Task<ClientModel> SendConfigRequest()
        {
            //《c#并发编程经典实例》 9.3 超时后取消
            var config = ClientConfig;
            Router.Logger.Debug("Reading Config From Provider..");
            TcpClient configClient = new TcpClient();
            configClient.NoDelay = true;//配置协议不使用nagle
            bool isConnected = false;
            bool isReconn = (this.ClientID != 0); //TODO XXX如果clientid已经分配到了id 则算作重连
            for (int j = 0; j < 3; j++) //连接服务端
            {
                var delayDispose = Task.Delay(TimeSpan.FromSeconds(Global.DefaultConnectTimeout)).ContinueWith(_ => configClient.Dispose());
                var connectAsync = configClient.ConnectAsync(config.ProviderAddress, config.ConfigPort);
                //超时则dispose掉
                var comletedTask = await Task.WhenAny(delayDispose, connectAsync);
                if (!connectAsync.IsCompleted) //超时
                {
                    Router.Logger.Debug("ReadConfigFromProvider连接超时，5秒后重试。");
                    await Task.Delay(5000);
                }
                else if (connectAsync.IsFaulted)//出错
                {
                    Router.Logger.Error(connectAsync.Exception.Message, connectAsync.Exception);
                    throw connectAsync.Exception;
                }
                else
                {
                    isConnected = true;
                    break;
                }
            }
            if (!isConnected) { Router.Logger.Debug("重试次数达到限制。"); throw new Exception("重试次数达到限制。"); }

            var configStream = configClient.GetStream();

            //请求0 协议名
            byte requestByte0;
            if (isReconn) requestByte0 = (byte)ServerProtocol.Reconnect;//重连则发送重连协议
            else requestByte0 = (byte)ServerProtocol.ClientNewAppRequest;

            await configStream.WriteAsync(new byte[] { requestByte0 }, 0, 1);

            //请求1 端口数
            var requestBytes = new ClientNewAppRequest
            {
                TokenLength = CurrentToken.Length,
                Token = CurrentToken,
                ClientId = this.ClientID,
                ClientCount = config.Clients.Count//(obj => obj.AppId == 0) //appid为0的则是未分配的 <- 取消这条规则，总是重新分配
                //Description = config.Description
            }.ToBytes();
            await configStream.WriteAsync(requestBytes, 0, requestBytes.Length);

            //请求2 分配端口
            //httpsupport: 增加host支持
            //TODO 7固定协议实现
            //port proto option(iscompress)  host         description   
            //2    1     1                   1024         96            
            int oneEndpointLength = 2 + 1 + 1 + 1024 + 96;//TODO 2 临时写的，这段需要重构
            byte[] requestBytes2 = new byte[config.Clients.Count * (oneEndpointLength)];
            int i = 0;
            foreach (var client in config.Clients)
            {
                byte[] portBytes = StringUtil.IntTo2Bytes(client.ConsumerPort);
                int offSetPos = oneEndpointLength * i;
                requestBytes2[offSetPos] = portBytes[0];        //端口
                requestBytes2[offSetPos + 1] = portBytes[1];    //端口
                requestBytes2[offSetPos + 2] = (byte)client.Protocol;//协议
                
                requestBytes2[offSetPos + 2 + 1] = (byte)(client.IsCompress ? 1 : 0);

                if (client.Host != null)                        //主机名
                    Encoding.ASCII.GetBytes(client.Host, 0, client.Host.Length, requestBytes2, offSetPos + 4);
                if (client.Description != null)
                {
                    Encoding.UTF8.GetBytes(client.Description, 0, client.Description.Length, requestBytes2, offSetPos + 4 + 1024);
                }

                i++;
            }
            await configStream.WriteAndFlushAsync(requestBytes2, 0, requestBytes2.Length);

            //高于1500左右需要考虑分帧断包的情况
            byte[] serverConfig = new byte[1024];

            //TODO 任何read都应该设置超时
            int readBytesCount = await configStream.ReadAsync(serverConfig, 0, serverConfig.Length);
            if (readBytesCount == 0)
                Router.Logger.Debug("服务器关闭了本次连接");//TODO 切换服务端时因为token的问题导致服务端无法使用
            else if (readBytesCount == -1)
                Router.Logger.Debug("连接超时");
            else if (readBytesCount == 1)
            {
                ServerStatus status = (ServerStatus)serverConfig[0];
                if (status == ServerStatus.AuthFailed)
                {
                    //验证失败，则删除当前服务器的缓存token
                    ClearLoginCache();
                    Router.Logger.Debug("校验失效，已清空登录缓存");
                    throw new Exception("校验失败");
                }
                else if (status == ServerStatus.UserBanned)
                {
                    Router.Logger.Debug("该用户被禁用");
                    throw new Exception("该用户被禁用");
                }
                else
                {
                    Router.Logger.Debug("服务端未知异常");
                    throw new Exception("服务端未知异常");
                }
            }

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
        public async Task PollingToProvider(Action<ClientStatus, List<string>> statusChanged, List<string> tunnelStrs)
        {
            var config = ClientConfig;
            if (ClientID == 0) { Router.Logger.Debug("error:未连接客户端"); return; };
            //int hungryNumber = MAX_CONNECT_SIZE / 2;
            byte[] clientBytes = StringUtil.IntTo2Bytes(ClientID);

            foreach (var kv in ServiceClientList)
            {

                int appid = kv.Key;
                await ConnectAppToServer(appid);
            }
            //TODO ***连接完成 回调客户端状态和连接隧道的状态
            statusChanged(ClientStatus.Started, tunnelStrs);
        }

        public async Task ConnectAppToServer(int appid)
        {
            var app = this.ServiceClientList[appid];
            var config = ClientConfig;
            // ClientAppWorker app = kv.Value;
            byte[] requestBytes = StringUtil.ClientIDAppIdToBytes(ClientID, appid);
            var clientList = new List<TcpClient>();
            //补齐
            var secureClient = (new TcpClient()).WrapClient(this.CurrentToken);//包装成客户端
            var client = secureClient.Client; 
            try
            {
                //1.连接服务端
                var state = await secureClient.ConnectWithAuthAsync(config.ProviderAddress, config.ReversePort);
                switch (state)
                {
                    case AuthState.Success:
                        Router.Logger.Debug("验证成功。");
                        break;
                    case AuthState.Fail:
                        Router.Logger.Debug("验证失败。");
                        //终止程序
                        return;
                    case AuthState.Error:
                        Router.Logger.Debug("校验出错。");
                        //终止程序
                        return;
                }

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

            app.Client = client;
            clientList.Add(client);
            //统一管理连接
            ConnectedConnections.AddRange(clientList);

            //事件循环1,这个方法必须放在最后 TODO 改为复数client，优化http请求
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

        /// <summary>
        /// 获取一个新的连接管理类
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static ServerConnectionManager Create(int clientId)
        {
            var scm = new ServerConnectionManager
            {
                _clientID = clientId
            };
            return scm;
        }

        /// <summary>
        /// 判断客户端是否还存在于连接列表当中，如果没有则说明是已被消费的连接，或者残留的无用的连接
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool ExistClient(int appId, TcpClient client)
        {
            if (ServiceClientList[appId].Client == null)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public bool RemoveClient(int appId, TcpClient client)
        {
            if (ServiceClientList[appId].Client == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void CloseAllConnections()
        {
            try
            {
                if (ServiceClientList != null)
                {
                    foreach (var kv in ServiceClientList)
                    {
                        if (kv.Value.Client != null && kv.Value.Client.Connected)
                            kv.Value.Client.Close();
                    }
                }

                if (ConnectedConnections != null)
                {
                    foreach (var conn in ConnectedConnections)
                    {
                        if (conn.Connected) conn.Close();
                    }
                }

                Router.Logger.Debug($"关闭反向链接：{ConnectedConnections?.Count}，关闭节点连接：{ServiceClientList?.Count}");

            }
            catch (Exception ex)
            {
                Router.Logger.Debug("关闭失败" + ex);
            }
        }

        public async Task StartHeartBeats(int interval, CancellationToken ct, TaskCompletionSource<object> waiter)
        {
            var timeStamp = Router.TimeStamp;
            try
            {
                var config = ClientConfig;

                //TODO 客户端开启心跳
                while (!ct.IsCancellationRequested)
                {
                    TcpClient client;
                    try
                    {
                        client = await NetworkUtil.ConnectAndSend(config.ProviderAddress,
                            config.ConfigPort, ServerProtocol.Heartbeat, StringUtil.IntTo2Bytes(this.ClientID));
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
                Router.Logger.Debug("心跳循环被取消。");
            }
            catch (Exception ex)
            {
                Router.Logger.Error("心跳连接出错:" + ex.Message, ex);
                return;
            }
            finally
            {
                Router.Logger.Debug("心跳循环已终止。");
                await Task.Delay(1000);
                //TODO 重启 需要进一步关注
                if (Router.TimeStamp == timeStamp)
                {
                    Router.Logger.Debug("心跳循环异常");
                    waiter.TrySetResult("心跳循环异常");
                }
            }

        }

        //TODO 3 清除特定的登录缓存
        public void ClearLoginCache()
        {
            //File.Delete(Router.NspClientCachePath);
            var clientUserCache = UserCacheManager.GetClientUserCache(Router.NspClientCachePath);
            clientUserCache.Remove(GetEndPoint());
            UserCacheManager.SaveChanges(Router.NspClientCachePath, clientUserCache);
        }

        public string GetEndPoint()
        {
            return ClientConfig.ProviderAddress + ":" + ClientConfig.ProviderWebPort;
        }


    }
}
