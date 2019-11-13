using NSmartProxy.Data;
using NSmartProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSmartProxy.Shared;
using System.IO;
using System.Reflection;
using NSmartProxy.Client.Authorize;
using NSmartProxy.ClientRouter.Dispatchers;
using NSmartProxy.Data.Models;

namespace NSmartProxy.Client
{
    public class NullLogger : INSmartLogger
    {
        public void Debug(object message)
        {
            //Not Implemented
        }

        public void Error(object message, Exception ex)
        {
            //Not Implemented
        }

        public void Info(object message)
        {
            //Not Implemented
        }
    }

    public enum ClientStatus
    {
        Stopped = 0,
        Started = 1,
        LoginError = 2
    }

    public class Router
    {
        private static string nspClientCachePath = null;

        public static string NSMART_CLIENT_CACHE_FILE = "cli_cache_v3.cache";

        private CancellationTokenSource ONE_LIVE_TOKEN_SRC;
        private CancellationTokenSource CANCEL_TOKEN_SRC;
        private CancellationTokenSource TRANSFERING_TOKEN_SRC;
        private CancellationTokenSource HEARTBEAT_TOKEN_SRC;
        private TaskCompletionSource<object> _waiter;
        private NSPDispatcher ClientDispatcher;
        //private UserCacheManager userCacheManager;

        public ServerConnectionManager ConnectionManager;
        public bool IsStarted = false;

        public Action DoServerNoResponse = delegate { };
        public Action<ClientStatus, List<string>> StatusChanged = delegate { };

        internal NSPClientConfig ClientConfig;
        internal LoginInfo CurrentLoginInfo;

        internal static INSmartLogger Logger = new NullLogger();   //inject
        internal static Guid TimeStamp; //时间戳，用来标识对象是否已经发生变化

        public static string NspClientCachePath
        {
            get
            {
                if (nspClientCachePath == null)
                {
                    //代表nsmartproxy.clientrouter.dll所在的路径
                    string assemblyFilePath = Assembly.GetExecutingAssembly().Location;
                    string assemblyDirPath = Path.GetDirectoryName(assemblyFilePath);
                    NspClientCachePath = assemblyDirPath + "\\" + NSMART_CLIENT_CACHE_FILE;
                    //NspClientCachePath = System.Environment.CurrentDirectory + "\\" + NSMART_CLIENT_CACHE_FILE;

                }

                return nspClientCachePath;
            }
            set => nspClientCachePath = value;
        }

        public Router()
        {
            ONE_LIVE_TOKEN_SRC = new CancellationTokenSource();
        }

        public Router(INSmartLogger logger) : this()
        {
            Logger = logger;
        }

        public Router SetConfiguration(NSPClientConfig config)//start之前一定要执行该方法，否则出错
        {
            ClientConfig = config;
            ClientDispatcher = new NSPDispatcher($"{ClientConfig.ProviderAddress}:{ClientConfig.ProviderWebPort}");
            return this;
        }

        public Router SetLoginInfo(LoginInfo loginInfo)
        {
            this.CurrentLoginInfo = loginInfo;
            return this;
        }

        /// <summary>
        /// 重要：连接服务端，一般做为入口方法
        /// 该方法主要操作一些配置和心跳
        /// AlwaysReconnect：始终重试，开启此选项，无论何时，一旦程序在连接不上时都会进行重试，否则只在连接成功后的异常中断时才重试。
        /// </summary>
        /// <returns></returns>
        public async Task Start(bool AlwaysReconnect = false, Action<ClientModel> complete = null)
        {
            if (AlwaysReconnect) IsStarted = true;
            var oneLiveToken = ONE_LIVE_TOKEN_SRC.Token;
            //登录功能
            string arrangedToken = Global.NO_TOKEN_STRING;

            while (!oneLiveToken.IsCancellationRequested)
            {
                CANCEL_TOKEN_SRC = new CancellationTokenSource();
                TRANSFERING_TOKEN_SRC = new CancellationTokenSource();
                HEARTBEAT_TOKEN_SRC = new CancellationTokenSource();
                _waiter = new TaskCompletionSource<object>();
                Router.TimeStamp = Guid.NewGuid();

                var appIdIpPortConfig = ClientConfig.Clients;
                int clientId = 0;

                //0 获取服务器端口配置
                try
                {
                    await InitServerPorts();
                }
                catch (Exception ex)//出错 重连
                {
                    if (IsStarted == false)
                    { StatusChanged(ClientStatus.LoginError, null); return; }
                    else
                    {
                        Logger.Error("获取服务器端口失败：" + ex.Message, ex);

                        await Task.Delay(Global.ClientReconnectInterval, ONE_LIVE_TOKEN_SRC.Token);
                        continue;
                    }
                }

                //0.5 处理登录/重登录/匿名登录逻辑
                try
                {
                    var clientUserCacheItem = UserCacheManager.GetUserCacheFromEndpoint(GetEndPoint(), NspClientCachePath);
                    //显式使用用户名密码登录
                    if (CurrentLoginInfo != null && CurrentLoginInfo.UserName != string.Empty)
                    {
                        var loginResult = await Login();
                        arrangedToken = loginResult.Item1;
                        clientId = loginResult.Item2;
                    }
                    else if (clientUserCacheItem != null)
                    {
                        //登录缓存
                        arrangedToken = clientUserCacheItem.Token;
                        //这个token的合法性无法保证,如果服务端删除了用户，而这里缓存还存在，会导致无法登录
                        //服务端校验token失效之后会主动关闭连接
                        CurrentLoginInfo = null;
                    }
                    else
                    {
                        //首次登录
                        //匿名登录，未提供登录信息时，使用空用户名密码自动注册并尝试匿名登录
                        Router.Logger.Debug("未提供登录信息，尝试匿名登录");
                        CurrentLoginInfo = new LoginInfo() { UserName = "", UserPwd = "" };
                        var loginResult = await Login();
                        arrangedToken = loginResult.Item1;
                        clientId = loginResult.Item2;
                        //保存缓存到磁盘

                        //File.WriteAllText(NspClientCachePath, arrangedToken);
                        UserCacheManager.UpdateUser(arrangedToken, "",
                            GetEndPoint(), NspClientCachePath);
                    }
                }
                catch (Exception ex)//出错 重连
                {
                    if (IsStarted == false)
                    { StatusChanged(ClientStatus.LoginError, null); return; }
                    else
                    {
                        Logger.Error("启动失败：" + ex.Message, ex);
                        await Task.Delay(Global.ClientReconnectInterval, ONE_LIVE_TOKEN_SRC.Token);
                        continue;
                    }
                }
                //1.获取配置
                ConnectionManager = ServerConnectionManager.Create(clientId);
                ConnectionManager.CurrentToken = arrangedToken;
                ConnectionManager.ClientGroupConnected += ServerConnnectionManager_ClientGroupConnected;
                ConnectionManager.ServerNoResponse = DoServerNoResponse;//下钻事件
                ClientModel clientModel = null;//
                try
                {
                    //从服务端初始化客户端配置
                    clientModel = await ConnectionManager.InitConfig(this.ClientConfig).ConfigureAwait(false);
                    complete?.Invoke(clientModel);
                }
                catch (Exception ex)
                {
                    //TODO 状态码：连接失败
                    Router.Logger.Error("连接失败：" + ex.Message, ex);
                    //throw;
                }

                //HasConnected = true;
                if (clientModel != null)
                {
                    int counter = 0;

                    //2.从服务端返回的appid上分配客户端的appid TODO 3 appid逻辑需要重新梳理
                    foreach (var app in appIdIpPortConfig)
                    {
                        //if (app.AppId == 0)
                        //{
                        app.AppId = clientModel.AppList[counter].AppId;
                        counter++;
                        //}
                    }
                    Logger.Debug("****************port list*************");
                    List<string> tunnelstrs = new List<string>();
                    foreach (var ap in clientModel.AppList)
                    {
                        var cApp = appIdIpPortConfig.First(obj => obj.AppId == ap.AppId);
                        //var cApp = appIdIpPortConfig[ap.AppId];
                        var tunnelStr = ap.AppId + ":  " + ClientConfig.ProviderAddress + ":" +
                                        ap.Port + "=>" +
                                        cApp.IP + ":" + cApp.TargetServicePort;
                        Logger.Debug(tunnelStr);
                        tunnelstrs.Add(tunnelStr);
                    }
                    Logger.Debug("**************************************");
                    _ = ConnectionManager.PollingToProvider(StatusChanged, tunnelstrs);
                    //3.创建心跳连接
                    _ = ConnectionManager.StartHeartBeats(Global.HeartbeatInterval, HEARTBEAT_TOKEN_SRC.Token, _waiter);

                    IsStarted = true;
                    if (await _waiter.Task.ConfigureAwait(false) is Exception exception)
                        Router.Logger.Debug($"程序异常终止:{exception.Message}。");
                    else Router.Logger.Debug($"未知异常。");
                }
                else
                {
                    Router.Logger.Debug($"程序启动失败。");
                    //如果程序从未启动过就出错，则终止程序，否则重试。
                    if (IsStarted == false) { StatusChanged(ClientStatus.Stopped, null); return; }
                }

                Router.Logger.Debug($"连接故障，尝试关闭连接并重试");
                if (ConnectionManager != null)
                    ConnectionManager.CloseAllConnections();//关闭所有连接
                //出错重试
                await Task.Delay(Global.ClientReconnectInterval, ONE_LIVE_TOKEN_SRC.Token);
                Router.Logger.Debug($"连接关闭，开启重试");
            }
            //正常终止
            Router.Logger.Debug($"停止重试，循环终止。");
        }


        /// <summary>
        /// 初始化服务器端口配置，并返回配置的dto
        /// </summary>
        /// <returns></returns>
        private async Task<ServerPortsDTO> InitServerPorts()
        {
            var result = await ClientDispatcher.GetServerPorts();
            if (result.State == 1)
            {
                ClientConfig.ReversePort = result.Data.ReversePort;
                ClientConfig.ConfigPort = result.Data.ConfigPort;
                Router.Logger.Debug($"配置端口：反向连接端口{ClientConfig.ReversePort},配置端口{ClientConfig.ConfigPort}");
                return result.Data;
            }
            else
            {

                throw new Exception("获取配置端口失败，服务端返回错误如下：" + result.Msg);
            }
        }

        private async Task<ValueTuple<string, int>> Login()
        {
            string arrangedToken;
            int clientId;
            //ClientDispatcher = new NSPDispatcher($"{ClientConfig.ProviderAddress}:{ClientConfig.ProviderWebPort}");
            var result = await ClientDispatcher.LoginFromClient(CurrentLoginInfo.UserName ?? "", CurrentLoginInfo.UserPwd ?? "");
            if (result.State == 1)
            {
                Router.Logger.Debug("登录成功");
                var data = result.Data;
                arrangedToken = data.Token;
                Router.Logger.Debug($"服务端版本号：{data.Version},当前适配版本号{NSPVersion.NSmartProxyServerName}");
                clientId = int.Parse(data.Userid);
                //File.WriteAllText(NspClientCachePath, arrangedToken);
                var endPoint = ClientConfig.ProviderAddress + ":" + ClientConfig.ProviderWebPort;
                UserCacheManager.UpdateUser(arrangedToken, CurrentLoginInfo.UserName, endPoint, NspClientCachePath);
            }
            else
            {

                throw new Exception("登录失败，服务端返回错误如下：" + result.Msg);
            }

            return (arrangedToken, clientId);
        }

        private void ServerConnnectionManager_ClientGroupConnected(object sender, EventArgs e)
        {
            var args = (ClientGroupEventArgs)e;
            foreach (TcpClient providerClient in args.NewClients)
            {
                Router.Logger.Debug("Open server connection.");
                _ = OpenTransmission(args.App.AppId, providerClient);
            }

        }

        /// <summary>
        /// 彻底关闭客户端并且不再重试
        /// </summary>
        /// <returns></returns>
        public async Task Close()
        {
            try
            {
                var config = ClientConfig;
                //客户端关闭
                CANCEL_TOKEN_SRC.Cancel();
                TRANSFERING_TOKEN_SRC.Cancel();
                HEARTBEAT_TOKEN_SRC.Cancel();
                ONE_LIVE_TOKEN_SRC.Cancel();
                _waiter.SetCanceled();
                //服务端关闭
                await NetworkUtil.ConnectAndSend(
                        config.ProviderAddress,
                    config.ConfigPort,
                        ServerProtocol.CloseClient,
                        StringUtil.IntTo2Bytes(this.ConnectionManager.ClientID),
                        true)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Router.Logger.Debug("关闭失败！" + ex);
            }
        }

        private async Task OpenTransmission(int appId, TcpClient providerClient)
        {
            TcpClient toTargetServer = new TcpClient();
            //事件循环2
            try
            {
                byte[] buffer = new byte[1];
                NetworkStream providerClientStream = providerClient.GetStream();
                //接收首条消息，首条消息中返回的是appid和客户端
                //消费端长连接，需要在server端保活
                try
                {
                    ControlMethod controlMethod;
                    //TODO 5 处理应用级的keepalive
                    while (true)
                    {
                        int readByteCount = await providerClientStream.ReadAsync(buffer, 0, buffer.Length); //双端标记S0001
                        if (readByteCount == 0)
                        {
                            //抛出错误以便上层重启客户端。
                            _waiter.TrySetResult(new Exception($"连接{appId}被服务器主动切断，已断开连接"));
                            return;

                        }

                        //TODO 4 如果是UDP则直接转发，之后返回上层
                        controlMethod = (ControlMethod)buffer[0];

                        switch (controlMethod)
                        {
                            case ControlMethod.KeepAlive: continue;
                            case ControlMethod.UDPTransfer:
                                await OpenUdpTransmission(); 
                                continue;//udp 发送后继续循环，方法里的ConnectAppToServer会再拉起一个新连接
                            case ControlMethod.TCPTransfer: 
                                await OpenTcpTransmission(appId, providerClient, toTargetServer); 
                                return;//tcp 开启隧道，并且不再利用此连接
                        }
                    } //while (controlMethod == ControlMethod.KeepAlive) ;
                }
                catch (Exception ex)
                {
                    //反弹连接出错为致命错误
                    //此处出错后，应用程序需要重置，并重启
                    _waiter.TrySetResult(ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("传输时出错：" + ex);
                //关闭传输连接
                toTargetServer.Close();
                providerClient.Close();
                throw;
            }

        }

        private async Task OpenUdpTransmission()
        {

        }

        private async Task OpenTcpTransmission(int appId, TcpClient providerClient, TcpClient toTargetServer)
        {
            //连接后设置client为null 
            if (ConnectionManager.ExistClient(appId, providerClient))
            {
                var removedClient = ConnectionManager.RemoveClient(appId, providerClient);
                if (removedClient == false)
                {
                    Router.Logger.Debug($"没有移除{appId}任何的对象，对象不存在. hash:{providerClient.GetHashCode()}");
                    return;
                }
            }
            else
            {
                Router.Logger.Debug($"已无法在{appId}中找到客户端 hash:{providerClient.GetHashCode()}.");
                return;
            }

            //每移除一个链接则发起一个新的链接
            Router.Logger.Debug(appId + "接收到连接请求");
            //根据clientid_appid发送到固定的端口
            //TODO 4 这里有性能隐患，考虑后期改成哈希表
            ClientApp item = ClientConfig.Clients.First((obj) => obj.AppId == appId);


            //向服务端再发起另一次长连接
            await ConnectionManager.ConnectAppToServer(appId);
            Router.Logger.Debug("已建立反向连接:" + appId);
            // item1:app编号，item2:ip地址，item3:目标服务端口
            try
            {
                toTargetServer.Connect(item.IP, item.TargetServicePort);
            }
            catch
            {
                throw new Exception($"对内网服务的 {item.IP}：{item.TargetServicePort} 连接失败。");
            }

            string epString = item.IP.ToString() + ":" + item.TargetServicePort.ToString();
            Router.Logger.Debug("已连接目标服务:" + epString);

            NetworkStream targetServerStream = toTargetServer.GetStream();
            NetworkStream providerClientStream = providerClient.GetStream();
            //targetServerStream.Write(buffer, 0, readByteCount);
            _ = TcpTransferAsync(providerClientStream, targetServerStream, providerClient, toTargetServer, epString, item);
        }


        private async Task TcpTransferAsync(NetworkStream providerStream, NetworkStream targetServceStream,
            TcpClient providerClient, TcpClient toTargetServer, string epString, ClientApp item)
        {
            try
            {
                Router.Logger.Debug("Looping start.");
                //创建相互转发流
                var taskT2PLooping = ToStaticTransfer(TRANSFERING_TOKEN_SRC.Token, targetServceStream, providerStream, epString, item);
                var taskP2TLooping = StreamTransfer(TRANSFERING_TOKEN_SRC.Token, providerStream, targetServceStream, epString, item);

                //close connnection,whether client or server stopped transferring.
                var comletedTask = await Task.WhenAny(taskT2PLooping, taskP2TLooping);
                //Router.Logger.Debug(comletedTask.Result + "传输关闭，重新读取字节");
                providerClient.Close();
                Router.Logger.Debug("已关闭toProvider连接。");
                toTargetServer.Close();
                Router.Logger.Debug("已关闭toTargetServer连接。");
            }
            catch (Exception ex)
            {
                Router.Logger.Debug(ex.ToString());
            }
        }


        private async Task StreamTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream,
            string epString, ClientApp item)
        {
            byte[] buffer = new byte[Global.ClientTunnelBufferSize];
            using (fromStream)
            {
                int bytesRead;
                while ((bytesRead =
                           await fromStream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) != 0)
                {
                    if (item.IsCompress)
                    {
                        var compressBuffer = StringUtil.DecompressInSnappy(buffer, 0, bytesRead);
                        bytesRead = compressBuffer.Length;
                        await toStream.WriteAsync(compressBuffer, 0, bytesRead, ct).ConfigureAwait(false);
                    }
                    else
                    {
                        //bytesRead = buffer.Length;
                        await toStream.WriteAsync(buffer, 0, bytesRead, ct).ConfigureAwait(false);
                    }

                    // await toStream.WriteAsync(buffer, 0, bytesRead, ct).ConfigureAwait(false);
                }
            }
            Router.Logger.Debug($"{epString}对节点传输关闭。");


        }


        private async Task ToStaticTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream,
            string epString, ClientApp item)
        {
            byte[] buffer = new byte[Global.ClientTunnelBufferSize];
            using (fromStream)
            {
                int bytesRead;
                while ((bytesRead =
                           await fromStream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) != 0)
                {
                    if (item.IsCompress)
                    {
                        var compressInSnappy = StringUtil.CompressInSnappy(buffer, 0, bytesRead);
                        var compressedBuffer = compressInSnappy.ContentBytes;
                        bytesRead = compressInSnappy.Length;
                        await toStream.WriteAsync(compressedBuffer, 0, bytesRead, ct).ConfigureAwait(false);
                    }
                    else
                    {
                        await toStream.WriteAsync(buffer, 0, bytesRead, ct).ConfigureAwait(false);
                    }
                }
            }
            Router.Logger.Debug($"{epString}反向链接传输关闭。");
        }

        private void SendZero(int port)
        {
            TcpClient tc = new TcpClient();
            tc.Connect("127.0.0.1", port);
            tc.Client.Send(new byte[] { 0x00 });
        }
        public string GetEndPoint()
        {
            return ClientConfig.ProviderAddress + ":" + ClientConfig.ProviderWebPort;
        }

    }

}
