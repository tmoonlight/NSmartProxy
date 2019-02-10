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

        public const int CLIENT_SERVER_PORT = 9973;
        public const int CONSUMER_PORT = 2344;
        //private TcpClient ProviderClient;
        //侦听serviceclient的端，始终存在
        //private List<TcpClient> S2PClients = new List<TcpClient>();
        public ClientConnectionManager ConnectionManager = null;
        public async Task Start()
        {
            ConnectionManager = ClientConnectionManager.GetInstance();
            //privider初始化
            //ProviderClient = new TcpClient();
            // List<TcpListener> serverClientListeners = new List<TcpListener>();
            CancellationTokenSource accepting = new CancellationTokenSource();

            TcpListener listenerConsumer = new TcpListener(IPAddress.Any, CONSUMER_PORT);

            TcpListener listenerServiceClient = new TcpListener(IPAddress.Any, CLIENT_SERVER_PORT);
            //while (true)
            //{
            //listenter初始化
            try
            {
                //一起打开listener，后面会先后进行accept
                //listenerServiceClient.Start(1000);
                listenerConsumer.Start(1000);

                Console.WriteLine("NSmart server started");
                // var taskResultClientService = AcceptClientServiceAsync(listenerServiceClient);
                //异步获取
                var taskResultConsumer = AcceptConsumeAsync(listenerConsumer, accepting.Token);

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
                listenerConsumer.Stop();
                listenerServiceClient.Stop();
            }
        }



        /// <summary>
        /// 同时侦听来自consumer的链接和到provider的链接
        /// </summary>
        /// <param name="consumerlistener"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        async Task AcceptConsumeAsync(TcpListener consumerlistener, CancellationToken ct)
        {

            //给两个listen，同时监听3端
            var clientCounter = 0;
            while (!ct.IsCancellationRequested)
            {
                //目标的代理服务联通了，才去处理consumer端的请求。
                Console.WriteLine("listening serviceClient....");
                TcpClient consumerClient = await consumerlistener.AcceptTcpClientAsync();
                //if (S2PClient.Connected == false)
                //{
                //    consumerClient.Close();
                //    // throw new Exception("未找到任何代理服务客户端。")
                //    Console.WriteLine("未找到对应的服务客户端,连接抛弃");
                //    continue;
                //}
                Console.WriteLine("consumer已连接");
                //连接成功 连接provider端
                clientCounter++;
                TcpClient s2pClient = ConnectionManager.GetClient();

                Task transferResult = TcpTransferAsync(consumerlistener, consumerClient, s2pClient, clientCounter, ct);

            }

        }

        //3端互相传输数据
        async Task TcpTransferAsync(TcpListener consumerlistener, TcpClient consumerClient, TcpClient providerClient,
            int clientIndex,
            CancellationToken ct)
        {
            Console.WriteLine("New client ({0}) connected", clientIndex);

            CancellationTokenSource transfering = new CancellationTokenSource();
            //转发buffer
            //连接C端
            //proxyClient.Connect("172.20.66.84", 80);

            var providerStream = providerClient.GetStream();
            var consumerStream = consumerClient.GetStream();
            Task taskC2PLooping = ToStaticTransfer(transfering.Token, consumerlistener, consumerClient, consumerStream, providerStream, "C2P", async (transbuf) =>
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
            Task taskP2CLooping = StreamTransfer(transfering.Token, providerClient, consumerClient, providerStream, consumerStream, "P2C", clientIndex);

            //任何一端传输中断或者故障，则关闭所有连接，回到上层重新accept
            var comletedTask = await Task.WhenAny(taskC2PLooping, taskP2CLooping);
            //comletedTask.
            Console.WriteLine("Transfering ({0}) STOPPED", clientIndex);
            consumerClient.Close();
            providerClient.Close();
            transfering.Cancel();
        }

        private async Task StreamTransfer(CancellationToken ct, TcpClient providerClient, TcpClient consumerClient, NetworkStream fromStream, NetworkStream toStream, string signal, int clientIndex, Func<byte[], Task<bool>> beforeTransfer = null)
        {

            var buf = new byte[4096];


            await fromStream.CopyToAsync(toStream, ct);

            Console.WriteLine("END WHILE???+++" + signal);
        }

        private async Task ToStaticTransfer(CancellationToken ct, TcpListener consumerlistener, TcpClient consumerClient, NetworkStream fromStream, NetworkStream toStream, string signal, Func<byte[], Task<bool>> beforeTransfer = null)
        {

            var buf = new byte[4096];


            await fromStream.CopyToAsync(toStream, ct);

            Console.WriteLine("END WHILE???+++" + signal);
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
    }
}