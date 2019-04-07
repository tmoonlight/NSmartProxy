using System;
using System.Buffers;
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
using NSmartProxy.Infrastructure;
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

        public static int ClientServicePort = 9973;   //服务端代理转发端口
        public static int ConfigServicePort = 12307;  //服务端配置通讯端口
        public static int WebManagementPort = 0;    //远端管理端口

        public ClientConnectionManager ConnectionManager = null;

        internal static INSmartLogger Logger; //inject

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
            //TODO 服务端心跳检测
            ProcessHeartbeatsCheck(Global.HeartbeatCheckInterval, ctsConsumer);

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

        private async Task ProcessHeartbeatsCheck(int interval, CancellationTokenSource cts)
        {
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    Server.Logger.Debug("开始心跳检测");
                    var outTimeClients = ConnectionManager.Clients.Where(
                        (cli) => DateTimeHelper.TimeRange(cli.LastUpdateTime, DateTime.Now) > interval).ToList();

                    foreach (var client in outTimeClients)
                    {
                        CloseAllSourceByClient(client.ClientID);
                    }
                    Server.Logger.Debug("结束心跳检测");
                    await Task.Delay(interval);
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            finally
            {
                Logger.Debug("fatal error:心跳检测处理异常终止。");
                //TODO 重新开始
            }
        }

        private void CloseAllSourceByClient(int clientId)
        {
            NSPClient client = ConnectionManager.Clients[clientId];
            string msg = "";
            foreach (var appKV in client.AppMap)
            {
                int port = appKV.Value.ConsumePort;
                //1.关闭，并移除AppMap中的App
                ConnectionManager.PortAppMap[port].Close();
                ConnectionManager.PortAppMap.Remove(port);
                msg += appKV.Value.ConsumePort + " ";
                //2.移除端口占用
                NetworkUtil.ReleasePort(port);
            }

            //3.移除client
            try
            {
                int closedClients = ConnectionManager.Clients.UnRegisterClient(client.ClientID);
                Server.Logger.Info(msg + $"已移除,{closedClients},个传输已终止。");
            }
            catch (Exception ex)
            {
                Server.Logger.Error($"CloseAllSourceByClient error:{ex.Message}", ex);
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

        /// <summary>
        /// 主循环，处理所有来自外部的请求
        /// </summary>
        /// <param name="consumerlistener"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        async Task ListenConsumeAsync(int consumerPort)
        {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            try
            {

                var consumerlistener = new TcpListener(IPAddress.Any, consumerPort);
                var nspApp = ConnectionManager.PortAppMap[consumerPort];
                consumerlistener.Start(1000);
                nspApp.Listener = consumerlistener;
                nspApp.CancelListenSource = cts;

                //临时编下号，以便在日志里区分不同隧道的连接
                var clientCounter = 0;
                while (!ct.IsCancellationRequested)
                {
                    Logger.Debug("listening serviceClient....Port:" + consumerPort);
                    //I.主要对外侦听循环
                    TcpClient consumerClient = await consumerlistener.AcceptTcpClientAsync();
                    clientCounter++;
                    ProcessConsumeRequestAsync(consumerPort, clientCounter, consumerClient, ct);
                }
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                cts.Cancel();
            }
        }

        private async Task ProcessConsumeRequestAsync(int consumerPort, int clientCounter, TcpClient consumerClient, CancellationToken ct)
        {
            TcpTunnel tunnel = new TcpTunnel();
            tunnel.ConsumerClient = consumerClient;
            ClientConnectionManager.GetInstance().PortAppMap[consumerPort].Tunnels.Add(tunnel);
            Logger.Debug("consumer已连接：" + consumerClient.Client.RemoteEndPoint.ToString());

            //II.弹出先前已经准备好的socket
            TcpClient s2pClient = await ConnectionManager.GetClient(consumerPort);

            tunnel.ClientServerClient = s2pClient;
            //✳关键过程✳
            //III.发送一个字节过去促使客户端建立转发隧道，至此隧道已打通
            //客户端接收到此消息后，会另外分配一个备用连接，此处异步发送性能较好
            s2pClient.GetStream().WriteAndFlushAsync(new byte[] { 1 }, 0, 1);

            await TcpTransferAsync(consumerClient, s2pClient, clientCounter, ct);
        }

        #region 配置连接相关
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

                int resultByte0 = await nstream.ReadAsync(protoRequestBytes);
                Protocol proto = (Protocol)protoRequestBytes[0];
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
                        //TODO 记录服务端更新时间
                        await ProcessHeartbeatProtocol(client);
                        break;
                    case Protocol.CloseClient:
                        await ProcessCloseClientProtocol(client);
                        break;
                    default:
                        throw new Exception("接收到异常请求。");
                        break;
                }

                //if (await ProcessAppRequestProtocol(client)) return;
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                throw;
            }

        }

        private async Task ProcessCloseClientProtocol(TcpClient client)
        {
            Server.Logger.Debug("Now processing CloseClient protocol....");
            NetworkStream nstream = client.GetStream();
            int closeClientLength = 2;
            byte[] appRequestBytes = new byte[closeClientLength];
            int resultByte = await nstream.ReadAsync(appRequestBytes);
            //Server.Logger.Debug("appRequestBytes received.");
            if (resultByte == 0)
            {
                CloseClient(client);
                return;
            }

            int clientID = StringUtil.DoubleBytesToInt(appRequestBytes[0], appRequestBytes[1]);
            //2.更新最后更新时间
            CloseAllSourceByClient(clientID);
            //3.接收完立即关闭
            client.Close();
        }

        private async Task ProcessHeartbeatProtocol(TcpClient client)
        {
            //1.读取clientID
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
            //1.2 响应ACK
            await nstream.WriteAndFlushAsync(new byte[] { 1 }, 0, 1);
            int clientID = StringUtil.DoubleBytesToInt(appRequestBytes[0], appRequestBytes[1]);

            //2.更新最后更新时间
            ConnectionManager.Clients[clientID].LastUpdateTime = DateTime.Now;
            //3.接收完立即关闭
            //client.Close();
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
            finally
            {
                client.Close();
            }

            ////4.给NSPClient关联configclient
            //nspClient.LastUpdateTime
            Logger.Debug("arrangedIds written.");

            return false;
        }

        #endregion


        #region datatransfer
        //3端互相传输数据
        async Task TcpTransferAsync(TcpClient consumerClient, TcpClient providerClient,
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
            //单独
            await fromStream.CopyToAsync(toStream, ct);

            //byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
            //try
            //{
            //    while (true)
            //    {
            //        int bytesRead = await fromStream.ReadAsync(new Memory<byte>(buffer), ct).ConfigureAwait(false);
            //        if (bytesRead == 0) break;
            //        await toStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), ct).ConfigureAwait(false);
            //    }
            //}
            //finally
            //{
            //    ArrayPool<byte>.Shared.Return(buffer);
            //}
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