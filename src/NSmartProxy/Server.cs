using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using NSmartProxy.Data;
using NSmartProxy.Extension;
using NSmartProxy.Infrastructure;
using NSmartProxy.Authorize;
using NSmartProxy.Data.Config;
using NSmartProxy.Data.DBEntities;
using NSmartProxy.Interfaces;
using NSmartProxy.Shared;
using static NSmartProxy.Server;
using NSmartProxy.Database;
using NSmartProxy.Infrastructure.Extension;

namespace NSmartProxy
{
    //+------------------------+
    //| NAT                    |
    //|                        |
    //|                        |
    //|    +----------+        |   +-----------+
    //|    |          |        |   |           |
    //|    |  Client  |------------>  Provider |
    //|    |          |        |   |           |
    //|    +----+-----+        |   +------^----+
    //|         |              |          |
    //|         |              |          |
    //|         |              |          |
    //|    +----V-----+        |          |
    //|    |          |        |          |
    //|    |  Target  |        |          |
    //|    |  Server  |        |          |
    //|    |          |        |   +------+-------+
    //|    +----------+        |   |              |
    //|                        |   |   Consumer   |
    //|                        |   |              |
    //+------------------------+   +--------------+
    public class Server
    {
        public const string USER_DB_PATH = "./nsmart_user.db";
        public const string SECURE_KEY_FILE_PATH = "./nsmart_sec_key";

        protected ClientConnectionManager ConnectionManager = null;
        protected IDbOperator DbOp;
        protected NSPServerContext ServerContext;

        internal static INSmartLogger Logger; //inject

        public Server(INSmartLogger logger)
        {
            //initialize
            Logger = logger;
            ServerContext = new NSPServerContext();
        }

        /// <summary>
        /// 设置服务端的配置文件，保存服务端配置时可以写入此文件
        /// </summary>
        /// <param name="configPath"></param>
        /// <returns></returns>
        public Server SetServerConfigPath(string configPath) //bad design 
        {
            ServerContext.ServerConfigPath = configPath;
            return this;
        }

        public Server SetAnonymousLogin(bool isSupportAnonymous)
        {
            ServerContext.SupportAnonymousLogin = isSupportAnonymous;
            return this;
        }

        public Server SetConfiguration(NSPServerConfig config)
        {
            ServerContext.ServerConfig = config;
            ServerContext.UpdatePortMap();
            return this;
        }

        //public static X509Certificate2 TestCert;

        public async Task Start()
        {
            DbOp = new LiteDbOperator(USER_DB_PATH);//加载数据库
            //从配置文件加载服务端配置
            InitSecureKey();
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            CancellationTokenSource ctsConfig = new CancellationTokenSource();
            CancellationTokenSource ctsHttp = new CancellationTokenSource();
            CancellationTokenSource ctsConsumer = new CancellationTokenSource();

            //1.反向连接池配置
            ConnectionManager = ClientConnectionManager.GetInstance().SetServerContext(ServerContext);
            //注册客户端发生连接时的事件
            ConnectionManager.AppTcpClientMapConfigApplied += ConnectionManager_AppAdded;
            _ = ConnectionManager.ListenServiceClient(DbOp);
            Logger.Debug("NSmart server started");

            //2.开启http服务
            if (ServerContext.ServerConfig.WebAPIPort > 0)
            {
                var httpServer = new HttpServer(Logger, DbOp, ServerContext, new HttpServerApis(ServerContext, DbOp, "./log"));
                _ = httpServer.StartHttpService(ctsHttp, ServerContext.ServerConfig.WebAPIPort);
            }

            //3.开启心跳检测线程 
            _ = ProcessHeartbeatsCheck(Global.HeartbeatCheckInterval, ctsConsumer);

            //3.5 加载SSL证书
            Logger.Debug("SSL CA Generating...");
            ServerContext.InitCertificates();
            Logger.Debug("SSL CA Generated.");
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
                DbOp.Close();
            }
        }

        private void InitSecureKey()
        {
            //生成密钥
            if (File.Exists(SECURE_KEY_FILE_PATH))
            {
                EncryptHelper.AES_Key = File.ReadAllText(SECURE_KEY_FILE_PATH);//prikey
            }
            else
            {
                EncryptHelper.AES_Key = RandomHelper.NextString(8);
                File.WriteAllText(SECURE_KEY_FILE_PATH, EncryptHelper.AES_Key);
            }
        }

        private async Task ProcessHeartbeatsCheck(int interval, CancellationTokenSource cts)
        {
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    //Server.Logger.Debug("开始心跳检测");
                    var outTimeClients = ServerContext.Clients.Where(
                        (cli) => DateTimeHelper.TimeRange(cli.LastUpdateTime, DateTime.Now) > interval).ToList();

                    foreach (var client in outTimeClients)
                    {
                        ServerContext.CloseAllSourceByClient(client.ClientID);
                    }
                    //Server.Logger.Debug("结束心跳检测");
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
            }
        }


        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Logger.Error(e.Exception.ToString(), e.Exception);
        }


        private async Task StartConfigService(CancellationTokenSource accepting)
        {
            TcpListener listenerConfigService = new TcpListener(IPAddress.Any, ServerContext.ServerConfig.ConfigPort);

            Logger.Debug("Listening config request on port " + ServerContext.ServerConfig.ConfigPort + "...");
            var taskResultConfig = AcceptConfigRequest(listenerConfigService);

            await taskResultConfig; //block here to hold open the server

        }

        /// <summary>
        /// 客户端连接时，会发送端口信息，此时服务端根据此信息
        /// 开始侦听新的端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectionManager_AppAdded(object sender, AppChangedEventArgs e)
        {
            Server.Logger.Debug("AppTcpClientMapConfigConnected");
            int port = 0;
            Dictionary<int, NSPAppGroup> appMap = null;
            if (e.App.AppProtocol == Protocol.UDP)
            {
                appMap = ServerContext.UDPPortAppMap;
            }
            else
            {
                appMap = ServerContext.PortAppMap;
            }
            //多个app共用一个端口时，只需要对这个端口开启一次侦听循环
            foreach (var kv in appMap)
            {
                if (kv.Value.ActivateApp.AppId == e.App.AppId &&
                    kv.Value.ActivateApp.ClientId == e.App.ClientId)
                {
                    port = kv.Value.ActivateApp.ConsumePort;
                    break;
                }
            }
            if (port == 0) throw new Exception("app未注册");

            _ = ListenConsumeAsync(port, appMap);
        }

        /// <summary>
        /// 主循环，处理所有来自外部的请求（UDP&TCP）
        /// </summary>
        /// <param name="consumerPort"></param>
        /// <param name="appMap"></param>
        /// <returns></returns>
        async Task ListenConsumeAsync(int consumerPort, Dictionary<int, NSPAppGroup> appMap)
        {
            //TODO 区分http请求
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            try
            {
                var nspAppGrp = appMap[consumerPort];
                nspAppGrp.ActivateApp.CancelListenSource = cts;
                if (nspAppGrp.ProtocolInGroup == Protocol.UDP)  //UDP协议侦听
                {
                    var consumerUdpClient = new UdpClient(consumerPort);
                    nspAppGrp.UdpClient = consumerUdpClient;

                    //启动udp传输，外层仅处理服务端到客户端的请求
                    while (!ct.IsCancellationRequested)
                    {
                        Logger.Debug("Server UDP Receiving....Port:" + consumerPort);
                        var receiveResult = await consumerUdpClient.ReceiveAsync();
                        _ = ProcessConsumeUdpRequestAsync(consumerUdpClient, consumerPort, receiveResult, ct);
                        //int x = 1;
                    }
                }
                else  //TCP HTTP/HTTPS都走tcplistener侦听
                {
                    var consumerTcpListener = new TcpListener(IPAddress.Any, consumerPort);
                    consumerTcpListener.Start(1000);
                    nspAppGrp.Listener = consumerTcpListener;
                    //临时编下号，以便在日志里区分不同隧道的连接
                    while (!ct.IsCancellationRequested)
                    {
                        Logger.Debug("Server TCP listening....Port:" + consumerPort);
                        //I.主要对外侦听循环
                        var consumerClient = await consumerTcpListener.AcceptTcpClientAsync();
                        _ = ProcessConsumeTcpRequestAsync(consumerPort, consumerClient, ct);//同步发送
                    }
                }
            }
            catch (ObjectDisposedException ode)
            {
                _ = ode;
                Logger.Debug($"外网端口{consumerPort}侦听时被外部终止");
            }
            catch (Exception ex)
            {
                Logger.Debug($"外网端口{consumerPort}侦听时出错{ex}");
            }
        }
        private object receiveUdpLocker = new object();
        ///接收到消费端的udp请求之后，开始侦测服务端->客户端的请求
        private async Task ProcessConsumeUdpRequestAsync(UdpClient consumerUdpClient, int consumerPort,
            UdpReceiveResult receiveResult,
            CancellationToken ct)
        {
            var nspAppGroup = ServerContext.UDPPortAppMap[consumerPort];
            if (nspAppGroup.ProtocolInGroup == Protocol.UDP)
            {
                try
                {
                    var s2pClient = ConnectionManager.GetClientForUdp(consumerPort, null);
                    var tunnelStream = s2pClient.GetStream();
                    var nspApp = nspAppGroup.ActivateApp;
                    // method   ip(D)    port      buffer(D)
                    // udp      X        2         X
                    tunnelStream.Write(new byte[] { (byte)ControlMethod.UDPTransfer }, 0, 1);
                    //tunnelStream.Write(StringUtil.IntTo2Bytes(nspApp.AppId));
                    //tunnelStream.Write(StringUtil.IntTo2Bytes(receiveResult.Buffer.Length), 0, 2);
                    await tunnelStream.WriteDLengthBytes(receiveResult.RemoteEndPoint.Address.ToString());
                    tunnelStream.Write(StringUtil.IntTo2Bytes(receiveResult.RemoteEndPoint.Port));
                    await tunnelStream.WriteDLengthBytes(receiveResult.Buffer);
                    Logger.Debug($"UDP数据包已发送{receiveResult.Buffer.Length}字节,remote ep:{receiveResult.RemoteEndPoint.ToString()}");
                    lock (receiveUdpLocker)
                    {
                        if (nspAppGroup.UdpTransmissionTask == null)//一个app只产生一个udp隧道（共用隧道）
                        {
                            //传回连接 (异步)
                            nspAppGroup.UdpTransmissionTask = OpenUdpTransmission(receiveResult.RemoteEndPoint, tunnelStream, nspAppGroup, ct);
                        }
                        //int x = 1;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug("UDP传输错误：" + ex.ToString());

                }

            }
        }

        /// <summary>
        /// udp的传输流，只需要处理客户端到服务端的数据流
        /// </summary>
        /// <param name="consumerUdpClient"></param>
        /// <param name="receiveResultRemoteEndPoint"></param>
        /// <param name="providerStream"></param>
        /// <param name="activateAppIsCompress"></param>
        /// <param name="ct"></param>
        /// <param name="consumerEndPoint"></param>
        /// <returns></returns>
        private async Task OpenUdpTransmission(IPEndPoint FXXXconsumerEndPoint,//这个endpoint没有意义
            NetworkStream providerStream, NSPAppGroup nspAppGrp, CancellationToken ct)
        {
            try
            {
                //byte[] buffer = new byte[Global.ServerTunnelBufferSize];
                //int bytesRead;
                var nspApp = nspAppGrp.ActivateApp;
                var udpClient = nspAppGrp.UdpClient;
                //var buffer = new byte[Global.ServerUdpBufferSize];

                while (true)
                {
                    //读取endpoint，必须保证如下数据包原子性
                    //IPv4/IPv6(D)         port    returnbuffer(D)
                    //X                    2       X
                    byte[] ipByte = await providerStream.ReadNextDLengthBytes();//read，(只会存活很短时间不是这个)
                    byte[] portByte = new byte[2];
                    byte[] returnBuffer = null;

                    await providerStream.ReadAsync(portByte, 0, 2);
                    returnBuffer = await providerStream.ReadNextDLengthBytes();

                    //if (readBytes > 0)
                    //{
                    _ = udpClient.SendAsync(returnBuffer,
                                   returnBuffer.Length,
                                   new IPEndPoint(IPAddress.Parse(Encoding.ASCII.GetString(ipByte)),
                                        StringUtil.DoubleBytesToInt(portByte)));
                    // }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("udp隧道传输失败");
                Logger.Debug(ex.ToString());
                throw;
            }


            //    }
            //while ((bytesRead =
            //           await providerStream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) != 0)
            //{
            //TODO 8 读取封包
            //读取客户端传过来的封包：consumer源 EndPoint 目的 EndPoint
            //new udpclient发送给相应的udpclient
            //if (isCompress)
            //{
            //    var compressBuffer = StringUtil.DecompressInSnappy(buffer, 0, bytesRead);
            //    bytesRead = compressBuffer.Length;

            //    await consumerUdpClient.SendAsync(compressBuffer, bytesRead, consumerEndPoint);
            //    //await toStream.WriteAsync(compressBuffer, 0, bytesRead, ct).ConfigureAwait(false);
            //}
            //else
            //{
            //    await consumerUdpClient.SendAsync(buffer, bytesRead, consumerEndPoint);
            //    //await toStream.WriteAsync(buffer, 0, bytesRead, ct).ConfigureAwait(false);
            //}
            //ServerContext.TotalSentBytes += bytesRead; //上行
            //}
        }


        private async Task ProcessConsumeTcpRequestAsync(int consumerPort, TcpClient consumerClient, CancellationToken ct)
        {
            TcpTunnel tunnel = new TcpTunnel { ConsumerClient = consumerClient };
            var nspAppGroup = ServerContext.PortAppMap[consumerPort];
            NSPApp nspApp = null;
            TcpClient s2pClient = null;
            Stream consumerStream = consumerClient.GetStream();
            Stream providerStream = null;
            byte[] restBytes = null;

            try
            {

                if (nspAppGroup.ProtocolInGroup == Protocol.HTTP /*|| nspAppGroup.ProtocolInGroup == Protocol.HTTPS*/)
                {//不论是http协议还是https协议，有证书就加密
                    if (ServerContext.PortCertMap.TryGetValue(consumerPort.ToString(), out X509Certificate cert2))
                    {
                        consumerStream = consumerStream.ProcessSSL(cert2);
                    }
                    var tp = await ReadHostName(consumerStream);

                    if (tp == null)
                    { Server.Logger.Debug("未在请求中找到主机名"); return; }

                    string host = tp.Item1;
                    restBytes = Encoding.UTF8.GetBytes(tp.Item2); //预发送bytes，因为这部分用来抓host消费掉了
                    //restBytesLength = tp.Item3;
                    s2pClient = await ConnectionManager.GetClientForTcp(consumerPort, host);
                    if (nspAppGroup.ContainsKey(host))
                    {
                        nspAppGroup[host].Tunnels.Add(tunnel); //bug修改：建立隧道
                    }
                    else
                    {
                        Server.Logger.Debug($"不存在host为“{host}”的主机名，但访问端依然以此主机名访问。");
                    }

                    nspApp = nspAppGroup[host];
                }
                else if (nspAppGroup.ProtocolInGroup == Protocol.TCP) //TCP
                {
                    s2pClient = await ConnectionManager.GetClientForTcp(consumerPort);
                    nspAppGroup.ActivateApp.Tunnels.Add(tunnel);//bug修改：建立隧道
                    nspApp = nspAppGroup.ActivateApp;
                }
            }
            catch (TimeoutException ex)
            {
                Logger.Debug($"端口{consumerPort}获取反弹连接超时，关闭传输。=>" + ex.Message);
                //获取clientid
                //关闭本次连接
                ServerContext.CloseAllSourceByClient(ServerContext.PortAppMap[consumerPort].ActivateApp.ClientId);
                return;
            }
            catch (KeyNotFoundException ex)
            {
                Server.Logger.Debug("未绑定此host：=>" + ex.Message);
                consumerClient.Close();
                return;
            }

            ServerContext.ConnectCount += 1;

            //TODO 如果NSPApp中是http，则需要进一步分离，通过GetHTTPClient来分出对应的client以建立隧道
            //II.弹出先前已经准备好的socket
            tunnel.ClientServerClient = s2pClient;
            CancellationTokenSource transfering = new CancellationTokenSource();
            //✳关键过程✳
            //III.发送一个字节过去促使客户端建立转发隧道，至此隧道已打通
            //客户端接收到此消息后，会另外分配一个备用连接

            //TODO 4 增加一个udp转发的选项
            providerStream = s2pClient.GetStream();
            //TODO 5 这里会出错导致无法和客户端通信
            try
            {
                await providerStream.WriteAndFlushAsync(new byte[] { (byte)ControlMethod.TCPTransfer }, 0, 1);//双端标记S0001
            }
            catch
            {
                Logger.Debug($"client:{nspApp.ClientId} app:{nspApp.AppId}写入失败,尝试切断该客户端");
                ServerContext.CloseAllSourceByClient(nspApp.ClientId);
                return;
            }

            //预发送bytes，因为这部分用来抓host消费掉了,所以直接转发
            if (restBytes != null)
                await providerStream.WriteAsync(restBytes, 0, restBytes.Length, transfering.Token);

            //TODO 4 TCP则打通隧道进行转发，UDP直接转发
            // if tcp
            try
            {
                await OpenTcpTransmission(consumerStream, providerStream, nspApp, transfering.Token);
            }
            finally
            {
                consumerClient.Close();
                s2pClient.Close();
                transfering.Cancel();
            }
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
                _ = ProcessConfigRequestAsync(client);
            }
        }

        private async Task ProcessConfigRequestAsync(TcpClient client)
        {
            try
            {
#if DEBUG
                //Server.Logger.Debug("config request received.");
#endif
                ServerContext.ClientConnectCount += 1;
                var nstream = client.GetStream();

                //0.读取协议名
                int protoRequestLength = 1;
                byte[] protoRequestBytes = new byte[protoRequestLength];

                int resultByte0 = await nstream.ReadAsync(protoRequestBytes, 0, protoRequestBytes.Length, Global.DefaultConnectTimeout);
                if (resultByte0 < 1)
                {
                    Server.Logger.Debug("服务端read失败，关闭连接");
                    client.Client.Close();
                    return;
                }

                ServerProtocol proto = (ServerProtocol)protoRequestBytes[0];

                switch (proto)
                {
                    case ServerProtocol.ClientNewAppRequest:
                        await ProcessAppRequestProtocol(client);
                        break;
                    case ServerProtocol.Heartbeat:
                        await ProcessHeartbeatProtocol(client);
                        break;
                    case ServerProtocol.CloseClient:
                        await ProcessCloseClientProtocol(client);
                        break;
                    case ServerProtocol.Reconnect:
                        await ProcessAppRequestProtocol(client, true);
                        break;
                    default:
                        throw new Exception("接收到异常请求。");
                }

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
            int resultByte = await nstream.ReadAsync(appRequestBytes, 0, appRequestBytes.Length, Global.DefaultConnectTimeout);
            //Server.Logger.Debug("appRequestBytes received.");
            if (resultByte < 1)
            {
                Server.Logger.Debug("服务端read失败，关闭连接");
                client.Client.Close();
                return;
            }

            int clientID = StringUtil.DoubleBytesToInt(appRequestBytes[0], appRequestBytes[1]);
            //2.更新最后更新时间
            ServerContext.CloseAllSourceByClient(clientID);
            //3.接收完立即关闭
            client.Close();
        }

        private async Task ProcessHeartbeatProtocol(TcpClient client)
        {
            //1.读取clientID

            NetworkStream nstream = client.GetStream();
            int heartBeatLength = 2;
            byte[] appRequestBytes = new byte[heartBeatLength];
            int resultByte = await nstream.ReadAsync(appRequestBytes, 0, appRequestBytes.Length, Global.DefaultConnectTimeout);
            if (resultByte < 1)
            {
                CloseClient(client);
                return;
            }

            int clientID = StringUtil.DoubleBytesToInt(appRequestBytes[0], appRequestBytes[1]);
            ServerContext.ClientConnectCount += 1;
            //2.更新最后更新时间
            if (ServerContext.Clients.ContainsKey(clientID))
            {
                //2.2 响应ACK 
                await nstream.WriteAndFlushAsync(new byte[] { (byte)ControlMethod.TCPTransfer });

                var nspClient = ServerContext.Clients[clientID];
                nspClient.LastUpdateTime = DateTime.Now;

                //2.3 TODO 5 发送保活数据包,一旦建立了这种机制就不需要iocontrol的keepalive了
                try
                {
                    //Server.Logger.Debug($"尝试对{clientID}发送保活数据包..");//TODO 待删除
                    foreach (var kv in nspClient.AppMap)
                    {
                        TcpClient peekedClient = kv.Value.TcpClientBlocks.Peek();
                        if (peekedClient != null)
                        {
                            //发送保活数据
                            await peekedClient.GetStream().WriteAndFlushAsync(new byte[] { (byte)ControlMethod.KeepAlive });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug($"{clientID}保活失败尝试切断连接");
                    throw ex;
                }

            }
            else
            {
                Server.Logger.Debug($"clientId为{clientID}客户端已经被清除。");
            }


            //3.接收完立即关闭,心跳协议无需使用nagle算法
            client.NoDelay = true;
            client.Close();
        }

        private async Task<bool> ProcessAppRequestProtocol(TcpClient client, bool IsReconnect = false)
        {
            Server.Logger.Debug("Now processing request protocol....");
            NetworkStream nstream = client.GetStream();
            int clientIdFromToken = 0;

            //1.读取配置请求1
            //TODO !!!!获取Token，截取clientID，校验
            //TODO !!!!这里的校验逻辑和httpserver_api存在太多重复，需要重构
            clientIdFromToken = await GetClientIdFromNextTokenBytes(client);
            if (clientIdFromToken == 0)
            {
                //TODO 2 服务端错误，校验失败
                await nstream.WriteAsync(new byte[] { (byte)ServerStatus.AuthFailed });
                client.Close();
                return false;
            }
            else if (clientIdFromToken == -1)
            {
                //用户被禁用
                await nstream.WriteAsync(new byte[] { (byte)ServerStatus.UserBanned });
                client.Close();
                return false;
            }

            //强制下线其他客户端
            ServerContext.CloseAllSourceByClient(clientIdFromToken, isForceClose: true);

            //1.3 获取客户端请求数
            int configRequestLength = 3;
            byte[] appRequestBytes = new byte[configRequestLength];
            int resultByte = await nstream.ReadAsyncEx(appRequestBytes);
            if (resultByte < 1)
            {
                CloseClient(client);
                return true;
            }

            Server.Logger.Debug("appRequestBytes received.");
            ServerContext.ClientConnectCount += 1;

            //2.根据配置请求1获取更多配置信息
            int appCount = (int)appRequestBytes[2];
            //port proto option(iscompress)  host         description   
            //2    1     1                   1024         96         
            byte[] consumerPortBytes = new byte[appCount * (2 + 1 + 1 + 1024 + 96)];//TODO 2 暂时这么写，亟需修改
            int resultByte2 = await nstream.ReadNextSTLengthBytes(consumerPortBytes);
            // int resultBytetest = await nstream.ReadAsyncEx(consumerPortBytes);
            //Server.Logger.Debug("consumerPortBytes received.");
            if (resultByte2 < 1)
            {
                Server.Logger.Debug("定长包没有正确接收");
                CloseClient(client);
                return true;
            }

            //NSPClient nspClient;
            //3.分配配置ID，并且写回给客户端
            try
            {
                byte[] arrangedIds = ConnectionManager.ArrangeConfigIds(appRequestBytes, consumerPortBytes, clientIdFromToken);
                Server.Logger.Debug("apprequest arranged");
                await nstream.WriteAsync(arrangedIds);
            }
            catch (Exception ex)
            {
                await nstream.WriteAsync(new byte[] { (byte)ServerStatus.UnknowndFailed });
                //TODO 2 服务端错误：未知错误
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

        /// <summary>
        /// 通过token获取clientid
        /// 返回0说明失败,返回-1说明用户被禁
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task<int> GetClientIdFromNextTokenBytes(TcpClient client)
        {
            NetworkStream nstream = client.GetStream();
            int clientIdFromToken = 0;
            //1.1 获取token长度
            int tokenLengthLength = 2;
            byte[] tokenLengthBytes = new byte[tokenLengthLength];
            int resultByte01 = await nstream.ReadAsyncEx(tokenLengthBytes);
            Server.Logger.Debug("tokenLengthBytes received.");
            if (resultByte01 < 1)
            {
                CloseClient(client);
                return 0;
            }

            //1.2 获取token
            int tokenLength = StringUtil.DoubleBytesToInt(tokenLengthBytes);
            byte[] tokenBytes = new byte[tokenLength];
            int resultByte02 = await nstream.ReadAsyncEx(tokenBytes);
            Server.Logger.Debug("tokenBytes received.");
            if (resultByte02 < 1)
            {
                CloseClient(client);
                return 0;
            }

            string token = tokenBytes.ToASCIIString();
            if (token != Global.NO_TOKEN_STRING)
            {
                var tokenClaims = StringUtil.ConvertStringToTokenClaims(token);
                var userJson = DbOp.Get(tokenClaims.UserKey);
                if (userJson == null)
                {
                    Server.Logger.Debug("token验证失败");
                }
                else
                {
                    var userId = userJson.ToObject<User>().userId;
                    if (ServerContext.ServerConfig.BoundConfig.UsersBanlist.Contains(userId))
                    {
                        Server.Logger.Debug("用户被禁用");
                        return -1;
                    }
                    else
                    {
                        clientIdFromToken = int.Parse(userId);
                    }
                }
            }

            return clientIdFromToken;
        }

        #endregion

        #region datatransfer
        //3端互相传输数据
        async Task OpenTcpTransmission(Stream consumerStream, Stream providerStream,
            NSPApp nspApp,
            CancellationToken ct)
        {
            try
            {
                Server.Logger.Debug($"New client ({nspApp.ClientId}-{nspApp.AppId}) connected");

                Task taskC2PLooping = ToStaticTransfer(ct, consumerStream, providerStream, nspApp);
                Task taskP2CLooping = StreamTransfer(ct, providerStream, consumerStream, nspApp);

                //任何一端传输中断或者故障，则关闭所有连接
                var comletedTask = await Task.WhenAny(taskC2PLooping, taskP2CLooping);
                Logger.Debug($"Transferring ({nspApp.ClientId}-{nspApp.AppId}) STOPPED");

            }
            catch (Exception e)
            {
                Logger.Debug(e);
                throw;
            }

        }

        private async Task StreamTransfer(CancellationToken ct, Stream fromStream, Stream toStream, NSPApp nspApp)
        {
            using (fromStream)
            {
                byte[] buffer = new byte[Global.ServerTunnelBufferSize];
                try
                {
                    int bytesRead;
                    while ((bytesRead =
                               await fromStream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) != 0)
                    {
                        if (nspApp.IsCompress)
                        {
                            var compressBuffer = StringUtil.DecompressInSnappy(buffer, 0, bytesRead);
                            bytesRead = compressBuffer.Length;
                            await toStream.WriteAsync(compressBuffer, 0, bytesRead, ct).ConfigureAwait(false);
                        }
                        else
                        {
                            await toStream.WriteAsync(buffer, 0, bytesRead, ct).ConfigureAwait(false);
                        }
                        ServerContext.TotalSentBytes += bytesRead; //上行
                    }
                }
                catch (Exception ioe)
                {
                    if (ioe is IOException) { return; } //Suppress this exception.
                    throw;
                }
            }
        }

        private async Task ToStaticTransfer(CancellationToken ct, Stream fromStream, Stream toStream, NSPApp nspApp)
        {
            using (fromStream)
            {
                byte[] buffer = new byte[Global.ServerTunnelBufferSize];
                try
                {
                    int bytesRead;
                    while ((bytesRead =
                               await fromStream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) != 0)
                    {
                        if (nspApp.IsCompress)
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
                        ServerContext.TotalReceivedBytes += bytesRead; //下行
                    }
                }
                catch (Exception ioe)
                {
                    if (ioe is IOException) { return; } //Suppress this exception.
                    throw;
                }
            }
        }

        private void CloseClient(TcpClient client)
        {
            if (client.Connected)
            {
                Logger.Debug("invalid request,Closing client:" + client.Client.RemoteEndPoint.ToString());
                client.Close();
                Logger.Debug("Closed client:" + client.Client.RemoteEndPoint.ToString());
            }
        }


        #endregion

        #region http
        private async Task<Tuple<string, string>> ReadHostName(Stream consumerStream)
        {
            //需要进一步截包 TODO 2 待优化1.内存优化 2.查询优化
            const int BUFFER_SIZE = 1024 * 1024 * 2;
            var length = 0;
            var data = string.Empty;
            var bytes = new byte[BUFFER_SIZE];

            do
            {//item2:data item1:host
                length = await consumerStream.ReadAsync(bytes, 0, BUFFER_SIZE);
                data += Encoding.UTF8.GetString(bytes, 0, length);
            } while (length > 0 && !data.Contains("\r\n\r\n"));

            if (length == 0) return null;
            Regex reg = new Regex("\r\nhost: (.*?)\r\n");

            return new Tuple<string, string>(
                reg.Match(data.ToLower()).Groups[1].Value,//需要进一步优化，使用list<byte[]>
                data

                );
        }
        #endregion

    }
}