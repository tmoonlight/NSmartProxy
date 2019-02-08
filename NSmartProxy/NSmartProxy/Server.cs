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
        //侦听serviceclient的端，始终存在
        private TcpClient S2PClient = new TcpClient();
        public async Task Start()
        {
            //privider初始化
            //ProviderClient = new TcpClient();
            // List<TcpListener> serverClientListeners = new List<TcpListener>();
            CancellationTokenSource accepting = new CancellationTokenSource();

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
                var taskResultClientService = AcceptClientServiceAsync(listenerServiceClient, accepting.Token);
                //异步获取
                var taskResultConsumer = AcceptConsumeAsync(listener, accepting.Token);

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
                listener.Stop();
                listenerServiceClient.Stop();
            }

            //}
        }



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


                Task transferResult = EchoAsync(consumerlistener, consumerClient, S2PClient, clientCounter, ct);

            }

        }

        //3端互相传输数据
        async Task EchoAsync(TcpListener consumerlistener, TcpClient consumerClient, TcpClient providerClient,
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
            transfering.Cancel();
        }

        private async Task StreamTransfer(CancellationToken ct, TcpClient providerClient, TcpClient consumerClient, NetworkStream fromStream, NetworkStream toStream, string signal, int clientIndex, Func<byte[], Task<bool>> beforeTransfer = null)
        {

            var buf = new byte[4096];

            //循环接收C并且写入A
            while (!ct.IsCancellationRequested)
            {
                //15秒没有心跳数据，则关闭连接释放资源
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(3600));
                //consumerStream.CopyTo(providerStream);//快速copy
                //2019-2-8 多端连接时，这里read出来的字符如何返回到相应的客户端
                var amountReadTask = fromStream.ReadAsync(buf, 0, buf.Length, ct);
                //var providerReadTask = stream.ReadAsync(providBuf, 0, providBuf.Length, ct);
                //15秒到了或者读取到了内容则进行<\X/>下一个时间片
                var completedTask = await Task.WhenAny(timeoutTask, amountReadTask);

                Console.WriteLine("clientIndex:" + clientIndex + ",token:" + ct.IsCancellationRequested.ToString()+",consuClientHash:"+consumerClient.GetHashCode()+",consuStreamHash:"+toStream.GetHashCode());
                //if (ct.IsCancellationRequested)
                //{ Console.WriteLine("强行终止，read了也不要");}

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
                    //throw;
                }

                // //※接收到来自iis的中断请求，切断consume的连接※
                if (amountRead == 0)
                {
                    //接收到0，则转发这个0给to端，并且中断这次传输
                    //await toStream.WriteAsync(buf, 0, amountRead, ct);
                    consumerClient.Close();
                    break;
                }//end of stream.

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

        private async Task ToStaticTransfer(CancellationToken ct, TcpListener consumerlistener, TcpClient consumerClient, NetworkStream fromStream, NetworkStream toStream, string signal, Func<byte[], Task<bool>> beforeTransfer = null)
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
                    //throw;
                }

                //※接收到来自浏览器的中断请求，转发这个请求，终止当前传输（不关闭prover-serviceclient的连接）※
                if (amountRead == 0)
                {
                    //接收到0，则中断consumer连接，转发这个0给to端，并且中断这次传输
                    fromStream.Close();
                    consumerClient.Close();
                    SendZero(CONSUMER_PORT);
                    // consumerlistener.Stop();

                    await toStream.WriteAsync(buf, 0, amountRead, ct);
                    break;
                }//end of stream.

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

        private void SendZero(int port)
        {
            TcpClient tc = new TcpClient();
            tc.Connect("127.0.0.1", port);
            tc.Client.Send(new byte[] { 0 });
        }
    }
}