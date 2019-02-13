using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        //固定端口，不会改变
        public const int CLIENT_SERVER_PORT = 9973;
        // public const int CONSUMER_PORT = 2344;
        public const int CONFIG_SERVICE_PORT = 12307;

        public ClientConnectionManager ConnectionManager = null;
        public async Task Start()
        {
            ConnectionManager = ClientConnectionManager.GetInstance();
            CancellationTokenSource accepting = new CancellationTokenSource();

            // TcpListener listenerConsumer = new TcpListener(IPAddress.Any, CONSUMER_PORT);

            TcpListener listenerServiceClient = new TcpListener(IPAddress.Any, CLIENT_SERVER_PORT);

            TcpListener listenerConfigService = new TcpListener(IPAddress.Any, CONFIG_SERVICE_PORT);

            try
            {


                Console.WriteLine("NSmart server started");
                var taskResultConfig = AcceptConfigRequest(listenerConfigService);

                //

                //get consumer client first.
                foreach (var kv in ConnectionManager.PortAppMap)
                {
                    //TcpTunnel tunnel = kv.Value;
                    var taskResultConsumer = AcceptConsumeAsync(kv.Key, accepting.Token);
                }


                await Task.Delay(6000000); //block here to hold open the server
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("all closed");
                accepting.Cancel();
                //listenerConsumer.Stop();
                listenerServiceClient.Stop();
            }
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
            while (1 == 1)
            {
                byte[] appRequestBytes = new byte[4];
                listenerConfigService.Start(100);
                var listener = await listenerConfigService.AcceptTcpClientAsync();
                var nstream = listener.GetStream();
                int resultByte = await nstream.ReadAsync(appRequestBytes);

                if (resultByte == 0)
                {
                    Console.WriteLine("invalid request");
                }

                byte[] arrangedIds = ConnectionManager.ArrageConfigIds(appRequestBytes);
                await nstream.WriteAsync(arrangedIds);
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
            var consumerlistener = new TcpListener(IPAddress.Any,consumerPort);
            consumerlistener.Start(1000);
            //给两个listen，同时监听3端
            var clientCounter = 0;
            while (!ct.IsCancellationRequested)
            {
                //目标的代理服务联通了，才去处理consumer端的请求。
                Console.WriteLine("listening serviceClient....");
                TcpClient consumerClient = await consumerlistener.AcceptTcpClientAsync();
                Console.WriteLine("consumer已连接");
                //连接成功 连接provider端
                clientCounter++;
                //需要端口
                TcpClient s2pClient = ConnectionManager.GetClient(consumerPort);

                Task transferResult = TcpTransferAsync(consumerlistener, consumerClient, s2pClient, clientCounter, ct);
            }
        }

        #region datatransfer
        //3端互相传输数据
        async Task TcpTransferAsync(TcpListener consumerlistener, TcpClient consumerClient, TcpClient providerClient,
            int clientIndex,
            CancellationToken ct)
        {
            Console.WriteLine("New client ({0}) connected", clientIndex);

            CancellationTokenSource transfering = new CancellationTokenSource();

            var providerStream = providerClient.GetStream();
            var consumerStream = consumerClient.GetStream();
            Task taskC2PLooping = ToStaticTransfer(transfering.Token, consumerStream, providerStream, async (transbuf) =>
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
            });
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


        private byte[] PartternWord = System.Text.Encoding.ASCII.GetBytes("GET /welcome/");
        private byte[] PartternPostWord = System.Text.Encoding.ASCII.GetBytes("POST /welcome/");

        //GET /welcome 
        private bool CompareBytes(byte[] wholeBytes, byte[] partternWord)
        {
            for (int i = 0; i < partternWord.Length; i++)
            {
                if (wholeBytes[i] != partternWord[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void SendZero(int port)
        {
            TcpClient tc = new TcpClient();
            tc.Connect("127.0.0.1", port);
            tc.Client.Send(new byte[] { 0 });
        }
        #endregion
    }
}