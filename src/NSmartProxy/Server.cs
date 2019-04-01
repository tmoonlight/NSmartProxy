using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using NSmartProxy.Data;
using NSmartProxy.Interfaces;
using NSmartProxy.Shared;
using static NSmartProxy.Server;

namespace NSmartProxy
{
    //+------------------------+
    //| NAT                    |
    //|                        |
    //|                        |
    //|    +----------+        |   +-----------+
    //|    |          |        |   |           |
    //|    |  client  |------------>  provider |
    //|    |          |        |   |           |
    //|    +----+-----+        |   +------^----+
    //|         |              |          |
    //|         |              |          |
    //|         |              |          |
    //|    +----V-----+        |          |
    //|    |          |        |          |
    //|    |   IIS    |        |          |
    //|    |          |        |          |
    //|    +----------+        |   +------+-------+
    //|                        |   |              |
    //|                        |   |   consumer   |
    //|                        |   |              |
    //+------------------------+   +--------------+
    public class Server
    {
        //服务端代理转发端口
        public static int ClientServicePort = 9973;
        //服务端配置通讯端口
        public static int ConfigServicePort = 12307;
        //远端管理端口
        public static int WebManagementPort = 0;

        public ClientConnectionManager ConnectionManager = null;

        //inject
        internal static INSmartLogger Logger;

        public Server(INSmartLogger logger)
        {
            Logger = logger;
        }

        //必须设置远程端口才可以通信
        public Server SetWebPort(int port)
        {
            WebManagementPort = port;
            return this;
        }

        public async Task Start()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            CancellationTokenSource ctsConfig = new CancellationTokenSource();
            CancellationTokenSource ctsHttp = new CancellationTokenSource();
            CancellationTokenSource ctsConsumer = new CancellationTokenSource();

            //1.反向连接池配置
            ConnectionManager = ClientConnectionManager.GetInstance();
            //注册客户端发生连接时的事件
            ConnectionManager.AppTcpClientMapConfigConnected += ConnectionManager_AppAdded;
            Logger.Debug("NSmart server started");

            //2.开启http服务
            if (WebManagementPort > 0)
            {
                var httpServer = new HttpServer(Logger);
                httpServer.StartHttpService(ctsHttp, WebManagementPort);
            }

            //3.开启心跳检测线程
            ProcessHeartbeats(Global.HeartbeatCheckInterval, ctsConsumer);

            //4.开启配置服务(常开)
            try
            {
                await StartConfigService(ctsConfig);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.Message);
            }
            finally
            {
                Logger.Debug("all closed");
                ctsConfig.Cancel(); ctsHttp.Cancel(); ctsConsumer.Cancel();
            }
        }

        private async Task ProcessHeartbeats(int interval, CancellationTokenSource cts)
        {
            try
            {
                String msg = "";
                while (!cts.Token.IsCancellationRequested)
                {
                    // List<int> pendingRemovekey = new List<int>();
                    var outTimeClients = ConnectionManager.Clients.Where(
                        (cli) => DateTime.Now.Ticks - cli.LastUpdateTime > interval).ToList();

                    foreach (var client in outTimeClients)
                    {
                        foreach (var appKV in client.AppMap)
                        {
                            int port = appKV.Value.ConsumePort;
                            //1.移除AppMap中的App
                            ConnectionManager.PortAppMap.Remove(port);
                            msg += appKV.Value.ConsumePort + " ";
                            //2.移除端口占用
                            NetworkUtil.ReleasePort(port);
                        }
                        //3.移除client
                        int closedClients = ConnectionManager.Clients.UnRegisterClient(client.ClientID);
                        Server.Logger.Info(msg + $"已移除,{closedClients},个传输已终止。");
                    }
                    // ConnectionManager.PortAppMap.Remove(port
                    await Task.Delay(interval);
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            finally
            {
                Logger.Debug("心跳检测异常终止。");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Logger.Error(e.Exception.ToString(), e.Exception);
        }




        private async Task StartConfigService(CancellationTokenSource accepting)
        {
            TcpListener listenerConfigService = new TcpListener(IPAddress.Any, ConfigServicePort);


            Logger.Debug("Listening config request on port " + ConfigServicePort.ToString() + "...");
            var taskResultConfig = AcceptConfigRequest(listenerConfigService);

            await taskResultConfig; //block here to hold open the server

        }

        /// <summary>
        /// 有连接连上则开始侦听新的端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectionManager_AppAdded(object sender, AppChangedEventArgs e)
        {
            Server.Logger.Debug("AppTcpClientMapReverseConnected事件已触发");
            int port = 0;
            foreach (var kv in ConnectionManager.PortAppMap)
            {
                if (kv.Value.AppId == e.App.AppId &&
                    kv.Value.ClientId == e.App.ClientId) port = kv.Key;
            }
            if (port == 0) throw new Exception("app未注册");
            var ct = new CancellationToken();

            ListenConsumeAsync(port);
        }

        #region 配置
        //配置服务，客户端可以通过这个服务接收现有的空闲端口
        //accept a config request.
        //request:
        //   2          1       1
        //  clientid    appid   nouse
        //
        //response:
        //   2          1       1  ...N
        //  clientid    appid   port
        private async Task AcceptConfigRequest(TcpListener listenerConfigService)
        {
            listenerConfigService.Start(100);
            while (true)
            {
                var client = await listenerConfigService.AcceptTcpClientAsync();
                ProcessConfigRequestAsync(client);
            }
        }

        private async Task ProcessConfigRequestAsync(TcpClient client)
        {
            try
            {
                Server.Logger.Debug("config request received.");
                var nstream = client.GetStream();

                //0.读取协议名
                int protoRequestLength = 1;
                byte[] protoRequestBytes = new byte[protoRequestLength];
                var proto = (Protocol)protoRequestBytes[0];
                int resultByte0 = await nstream.ReadAsync(protoRequestBytes);
                Server.Logger.Debug("appRequestBytes received.");
                if (resultByte0 == 0)
                {
                    CloseClient(client);
                    return;
                }

                switch (proto)
                {
                    case Protocol.ClientNewAppRequest:
                        await ProcessAppRequestProtocol(client);
                        break;
                    case Protocol.Heartbeat:
                        //TODO 重启计时器
                        await ProcessHeartbeatProtocol(client);
                        break;
                    default:
                        throw new Exception("接收到异常请求。");
                        break;
                }

                if (await ProcessAppRequestProtocol(client)) return;
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                throw;
            }

        }

        private async Task ProcessHeartbeatProtocol(TcpClient client)
        {
            Server.Logger.Debug("Now processing Heartbeat protocol....");
            NetworkStream nstream = client.GetStream();
            int heartBeatLength = 2;
            byte[] appRequestBytes = new byte[heartBeatLength];
            int resultByte = await nstream.ReadAsync(appRequestBytes);
            //Server.Logger.Debug("appRequestBytes received.");
            if (resultByte == 0)
            {
                CloseClient(client);
                return;
            }

            int clientID = StringUtil.DoubleBytesToInt(appRequestBytes[0], appRequestBytes[1]);
            ConnectionManager.Clients[clientID].LastUpdateTime = DateTime.Now.Ticks;
        }

        private async Task<bool> ProcessAppRequestProtocol(TcpClient client)
        {
            Server.Logger.Debug("Now processing request protocol....");
            NetworkStream nstream = client.GetStream();
            //1.读取配置请求1
            int configRequestLength = 3;
            byte[] appRequestBytes = new byte[configRequestLength];
            int resultByte = await nstream.ReadAsync(appRequestBytes);
            Server.Logger.Debug("appRequestBytes received.");
            if (resultByte == 0)
            {
                CloseClient(client);
                return true;
            }

            //2.根据配置请求1获取更多配置信息
            int appCount = (int)appRequestBytes[2];
            byte[] consumerPortBytes = new byte[appCount * 2];
            int resultByte2 = await nstream.ReadAsync(consumerPortBytes);
            Server.Logger.Debug("consumerPortBytes received.");
            if (resultByte2 == 0)
            {
                CloseClient(client);
                return true;
            }

            //NSPClient nspClient;
            //3.分配配置ID，并且写回给客户端
            try
            {
                byte[] arrangedIds = ConnectionManager.ArrageConfigIds(appRequestBytes, consumerPortBytes);
                Server.Logger.Debug("apprequest arranged");
                await nstream.WriteAsync(arrangedIds);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.ToString());
            }

            ////4.给NSPClient关联configclient
            //nspClient.LastUpdateTime


            Logger.Debug("arrangedIds written.");
            return false;
        }

        #endregion


        /// <summary>
        /// 同时侦听来自consumer的链接和到provider的链接
        /// </summary>
        /// <param name="consumerlistener"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        async Task ListenConsumeAsync(int consumerPort)
        {
            try
            {
                var cts = new CancellationTokenSource();
                var consumerlistener = new TcpListener(IPAddress.Any, consumerPort);
                var nspApp = ConnectionManager.PortAppMap[consumerPort];
                var ct = cts.Token;
                consumerlistener.Start(1000);
                //Token,Listener加入全局管理
                nspApp.Listener = consumerlistener;
                nspApp.CancelListenSource = cts;

                //给两个listener，同时监听3端
                var clientCounter = 0;
                while (!ct.IsCancellationRequested)
                {
                    //目标的代理服务联通了，才去处理consumer端的请求。
                    Logger.Debug("listening serviceClient....Port:" + consumerPort);
                    TcpClient consumerClient = await consumerlistener.AcceptTcpClientAsync();
                    //记录tcp隧道，消费端
                    TcpTunnel tunnel = new TcpTunnel();
                    tunnel.ConsumerClient = consumerClient;
                    ClientConnectionManager.GetInstance().PortAppMap[consumerPort].Tunnels.Add(tunnel);
                    Logger.Debug("consumer已连接：" + consumerClient.Client.RemoteEndPoint.ToString());
                    //消费端连接成功,连接

                    //TODO 此处是否新开线程异步处理？
                    //需要端口
                    TcpClient s2pClient = await ConnectionManager.GetClient(consumerPort);
                    //记录tcp隧道，客户端
                    tunnel.ClientServerClient = s2pClient;
                    //✳关键过程✳
                    //连接完之后发送一个字节过去促使客户端建立转发隧道
                    await s2pClient.GetStream().WriteAsync(new byte[] { 1 }, 0, 1);
                    clientCounter++;

                    TcpTransferAsync(consumerlistener, consumerClient, s2pClient, clientCounter, ct);
                }
            }
            catch (Exception e)
            {
                Logger.Debug(e);
            }

        }

        #region datatransfer
        //3端互相传输数据
        async Task TcpTransferAsync(TcpListener consumerlistener, TcpClient consumerClient, TcpClient providerClient,
            int clientIndex,
            CancellationToken ct)
        {
            try
            {
                Server.Logger.Debug($"New client ({clientIndex}) connected");

                CancellationTokenSource transfering = new CancellationTokenSource();

                var providerStream = providerClient.GetStream();
                var consumerStream = consumerClient.GetStream();
                Task taskC2PLooping = ToStaticTransfer(transfering.Token, consumerStream, providerStream);
                Task taskP2CLooping = StreamTransfer(transfering.Token, providerStream, consumerStream);

                //任何一端传输中断或者故障，则关闭所有连接
                var comletedTask = await Task.WhenAny(taskC2PLooping, taskP2CLooping);
                //comletedTask.
                Logger.Debug($"Transferring ({clientIndex}) STOPPED");
                consumerClient.Close();
                providerClient.Close();
                transfering.Cancel();
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                throw;
            }

        }

        private async Task StreamTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream)
        {
            await fromStream.CopyToAsync(toStream, ct);
        }

        private async Task ToStaticTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream, Func<byte[], Task<bool>> beforeTransferHandle = null)
        {
            await fromStream.CopyToAsync(toStream, ct);
        }

        private void CloseClient(TcpClient client)
        {
            Logger.Debug("invalid request,Closing client:" + client.Client.RemoteEndPoint.ToString());
            client.Close();
            Logger.Debug("Closed client:" + client.Client.RemoteEndPoint.ToString());
        }



        #endregion


    }
}