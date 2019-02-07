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
    //|    |  server  <------------+   proxy   |
    //|    |          |        |   |           |
    //|    +----------+        |   +------^----+
    //|                        |          |
    //|                        |          |
    //|                        |          |
    //|    +----------+        |          |
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
        public async Task Start()
        {
            //privider初始化
            //ProviderClient = new TcpClient();
            // List<TcpListener> serverClientListeners = new List<TcpListener>();
            CancellationTokenSource cts = new CancellationTokenSource();
            TcpListener listener = new TcpListener(IPAddress.Any, CONSUMER_PORT);

            TcpListener listenerServiceClient = new TcpListener(IPAddress.Any, CLIENT_SERVER_PORT);
            //while (true)
            //{
            //listenter初始化
            try
            {
                //一起打开listener，后面会先后进行accept
                listenerServiceClient.Start(1000);
                listener.Start(1000);

                Console.WriteLine("NSmart server started");
                var taskResultClientService = AcceptClientServiceAsync(listenerServiceClient, cts.Token);
                //异步获取
                var taskResultConsumer = AcceptConsumeAsync(listener, cts.Token);

                await Task.Delay(6000000); //block here to hold open the server
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("all closed");
                cts.Cancel();
                listener.Stop();
                listenerServiceClient.Stop();
            }

            //}
        }

        private TcpClient S2PClient = new TcpClient();

        //刷出serviceclient到provider这一条通道
        async Task AcceptClientServiceAsync(TcpListener serviceClientListener, CancellationToken ct)
        {
            while (1 == 1)
            {

                TcpClient tempS2PClient = await serviceClientListener.AcceptTcpClientAsync();

                Console.WriteLine("传入新的clientserver连接");
                if (S2PClient.Connected == false)
                {
                    S2PClient = tempS2PClient;
                }
                Console.WriteLine("S2PClient存在");
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
                if (S2PClient.Connected == false)
                {
                    consumerClient.Close();
                    // throw new Exception("未找到任何代理服务客户端。")
                    Console.WriteLine("未找到对应的服务客户端,连接抛弃");
                    continue;
                }
                Console.WriteLine("consumer已连接");



                //连接成功 连接provider端
                clientCounter++;


                Task transferResult = EchoAsync(consumerClient, S2PClient, clientCounter, ct);

            }

        }

        //3端互相传输数据
        async Task EchoAsync(TcpClient consumerClient, TcpClient providerClient,
            int clientIndex,
            CancellationToken ct)
        {
            Console.WriteLine("New client ({0}) connected", clientIndex);
            //转发buffer
            //连接C端
            //proxyClient.Connect("172.20.66.84", 80);

            var providerStream = providerClient.GetStream();
            var consumerStream = consumerClient.GetStream();
            Task taskC2PLooping = StreamTransfer(ct, consumerStream, providerStream, "C2P", async (transbuf) =>
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
            Task taskP2CLooping = StreamTransfer(ct, providerStream, consumerStream, "P2C");


            //循环接受A并写入C
            var comletedTask = await Task.WhenAny(taskC2PLooping, taskP2CLooping);
            //comletedTask.
            Console.WriteLine("Client ({0}) disconnected", clientIndex);
        }

        private async Task StreamTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream, string signal, Func<byte[], Task<bool>> beforeTransfer = null)
        {

            var buf = new byte[4096];

            //循环接收C并且写入A
            while (!ct.IsCancellationRequested)
            {
                //15秒没有心跳数据，则关闭连接释放资源
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(3600));
                //consumerStream.CopyTo(providerStream);//快速copy
                var amountReadTask = fromStream.ReadAsync(buf, 0, buf.Length, ct);
                //var providerReadTask = stream.ReadAsync(providBuf, 0, providBuf.Length, ct);
                //15秒到了或者读取到了内容则进行<\X/>下一个时间片
                var completedTask = await Task.WhenAny(timeoutTask, amountReadTask);

                // 非windowsform不需要 .ConfigureAwait(false);
                if (completedTask == timeoutTask)
                {
                    // var msg = Encoding.ASCII.GetBytes("consumer timed out");
                    Console.WriteLine("proxy transfer timed out");

                    break;
                }

                //在接收到信息之后可以立即发送一些消息给客户端。
                //获取read之后返回结果（结果串长度）

                var amountRead = 0;
                try
                {
                    amountRead = amountReadTask.Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
              
                if (amountRead == 0) break; //end of stream.

                bool continueWrite = true;
                if (beforeTransfer != null)
                {
                    continueWrite = await beforeTransfer(buf);
                }

                if (continueWrite)
                {
                    //转发
                    await toStream.WriteAsync(buf, 0, amountRead, ct);
                }
            }
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
    }
}