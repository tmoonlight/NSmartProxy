using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSmartProxy.Interfaces;

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
            CancellationTokenSource ctsConfig = new CancellationTokenSource();
            CancellationTokenSource ctsHttp = new CancellationTokenSource();

            //1.开启客户端连接服务
            ConnectionManager = ClientConnectionManager.GetInstance();
            //注册客户端发生连接时的事件
            ConnectionManager.AppAdded += ConnectionManager_AppAdded;
            Console.WriteLine("NSmart server started");

            //2.开启http服务
            if (WebManagementPort > 0)
                StartHttpService(ctsHttp);

            //3.开启配置服务

            try
            {
                await StartConfigService(ctsConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("all closed");
                ctsConfig.Cancel();
                //listenerConsumer.Stop();
            }


        }

        #region HTTPServer
        private async Task StartHttpService(CancellationTokenSource ctsHttp)
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add($"http://127.0.0.1:{WebManagementPort}/");
                //TcpListener listenerConfigService = new TcpListener(IPAddress.Any, WebManagementPort);
                Console.WriteLine("Listening HTTP request on port " + WebManagementPort.ToString() + "...");
                await AcceptHttpRequest(listener, ctsHttp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Server.Logger.Error(ex.ToString(), ex);
            }
        }

        private async Task AcceptHttpRequest(HttpListener httpService, CancellationTokenSource ctsHttp)
        {
            httpService.Start();
            while (true)
            {
                var client = await httpService.GetContextAsync();
                ProcessHttpRequestAsync(client);
            }
        }

        private async Task ProcessHttpRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "text/html;charset=utf-8";

            //getJson
            StringBuilder json = new StringBuilder("[");
            foreach (var ca in this.ConnectionManager.PortAppMap)
            {
                json.Append("{");
                json.Append(KV2Json("port", ca.Key)).C();
                json.Append(KV2Json("clientId", ca.Value.ClientID)).C();
                json.Append(KV2Json("appId", ca.Value.AppID)).C();

                
                json.Append(KV2Json("connections"));
                json.Append("[");
                foreach (TcpClient client in ConnectionManager.AppTcpClientMap[ca.Value])
                {
                    json.Append("{");
                    json.Append(KV2Json("lEndPoint", client.Client.LocalEndPoint.ToString())).C();
                    json.Append(KV2Json("rEndPoint", client.Client.RemoteEndPoint.ToString()));
                    //json.Append(KV2Json("p", c)).C();
                    //json.Append(KV2Json("port", ca.Key));
                    json.Append("}");
                    json.C();
                }
                json.D();
                json.Append("]");
                json.Append("}").C();
            }
            json.D();
            json.Append("]");
            await response.OutputStream.WriteAsync(HtmlUtil.GetContent(json.ToString()));
            //await response.OutputStream.WriteAsync(HtmlUtil.GetContent(request.RawUrl));
            response.OutputStream.Close();
        }

        private string KV2Json(string key)
        {
            return "\"" + key + "\":";
        }
        private string KV2Json(string key, object value)
        {
            return "\"" + key + "\":\"" + value.ToString() + "\"";
        }

        #endregion


        private async Task StartConfigService(CancellationTokenSource accepting)
        {
            TcpListener listenerConfigService = new TcpListener(IPAddress.Any, ConfigServicePort);


            Console.WriteLine("Listening config request on port " + ConfigServicePort.ToString() + "...");
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
            Server.Logger.Debug("added事件已触发");
            int port = 0;
            foreach (var kv in ConnectionManager.PortAppMap)
            {
                if (kv.Value.AppID == e.App.AppID &&
                    kv.Value.ClientID == e.App.ClientID) port = kv.Key;
            }
            if (port == 0) throw new Exception("app未注册");
            var ct = new CancellationToken();
            Task tsk = AcceptConsumeAsync(port, ct);
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
                byte[] appRequestBytes = new byte[4];
                Server.Logger.Debug("config request received.");
                var nstream = client.GetStream();
                int resultByte = await nstream.ReadAsync(appRequestBytes);
                Server.Logger.Debug("appRequestBytes received.");
                if (resultByte == 0)
                {
                    Console.WriteLine("invalid request");
                }

                try
                {
                    byte[] arrangedIds = ConnectionManager.ArrageConfigIds(appRequestBytes);
                    Server.Logger.Debug("apprequest arranged");
                    await nstream.WriteAsync(arrangedIds);
                }
                catch (Exception ex)
                { Console.WriteLine(ex.ToString()); }


                Console.WriteLine("arrangedIds written.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
        #endregion


        /// <summary>
        /// 同时侦听来自consumer的链接和到provider的链接
        /// </summary>
        /// <param name="consumerlistener"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        async Task AcceptConsumeAsync(int consumerPort, CancellationToken ct)
        {
            var consumerlistener = new TcpListener(IPAddress.Any, consumerPort);
            consumerlistener.Start(1000);
            //给两个listen，同时监听3端
            var clientCounter = 0;
            while (!ct.IsCancellationRequested)
            {
                //目标的代理服务联通了，才去处理consumer端的请求。
                Console.WriteLine("listening serviceClient....Port:" + consumerPort);
                TcpClient consumerClient = await consumerlistener.AcceptTcpClientAsync();
                Console.WriteLine("consumer已连接");
                //连接成功 连接provider端


                //需要端口
                TcpClient s2pClient = ConnectionManager.GetClient(consumerPort);
                //✳关键过程✳
                //连接完之后发送一个字节过去促使客户端建立转发隧道
                await s2pClient.GetStream().WriteAsync(new byte[] { 1 }, 0, 1);
                clientCounter++;

                Task transferResult = TcpTransferAsync(consumerlistener, consumerClient, s2pClient, clientCounter, ct);
            }
        }

        #region datatransfer
        //3端互相传输数据
        async Task TcpTransferAsync(TcpListener consumerlistener, TcpClient consumerClient, TcpClient providerClient,
            int clientIndex,
            CancellationToken ct)
        {
            Server.Logger.Debug(string.Format("New client ({0}) connected", clientIndex));

            CancellationTokenSource transfering = new CancellationTokenSource();

            var providerStream = providerClient.GetStream();
            var consumerStream = consumerClient.GetStream();
            Task taskC2PLooping = ToStaticTransfer(transfering.Token, consumerStream, providerStream /*, async (transbuf) =>
            {
                if (CompareBytes(transbuf, PartternWord))
                {
                    var contentBytes = HtmlUtil.GetUtf8Content(transbuf);
                    await consumerStream.WriteAsync(contentBytes, 0, contentBytes.Length, ct);
                    consumerStream.Flush();
                    consumerStream.Close();
                    return false;
                }
                else return true;
            }*/);
            Task taskP2CLooping = StreamTransfer(transfering.Token, providerStream, consumerStream);

            //任何一端传输中断或者故障，则关闭所有连接，回到上层重新accept
            var comletedTask = await Task.WhenAny(taskC2PLooping, taskP2CLooping);
            //comletedTask.
            Console.WriteLine("Transfering ({0}) STOPPED", clientIndex);
            consumerClient.Close();
            providerClient.Close();
            transfering.Cancel();
        }

        private async Task StreamTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream)
        {
            await fromStream.CopyToAsync(toStream, ct);
        }

        private async Task ToStaticTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream, Func<byte[], Task<bool>> beforeTransferHandle = null)
        {
            await fromStream.CopyToAsync(toStream, ct);
        }


        //private byte[] PartternWord = System.Text.Encoding.ASCII.GetBytes("GET /welcome/");
        //private byte[] PartternPostWord = System.Text.Encoding.ASCII.GetBytes("POST /welcome/");

        ////GET /welcome 
        //private bool CompareBytes(byte[] wholeBytes, byte[] partternWord)
        //{
        //    for (int i = 0; i < partternWord.Length; i++)
        //    {
        //        if (wholeBytes[i] != partternWord[i])
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        //private void SendZero(int port)
        //{
        //    TcpClient tc = new TcpClient();
        //    tc.Connect("127.0.0.1", port);
        //    tc.Client.Send(new byte[] { 0 });
        //}
        #endregion


    }
}