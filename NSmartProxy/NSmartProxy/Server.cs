using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSmartProxy
{
    public class Server
    {
        //private TcpClient ProviderClient;
        public async Task Start()
        {
            //privider初始化
            //ProviderClient = new TcpClient();


            //listenter初始化
            CancellationTokenSource cts = new CancellationTokenSource();
            TcpListener listener = new TcpListener(IPAddress.Any, 6666);
            try
            {
                listener.Start();
                Console.WriteLine("NSmart server started");
                //just fire and forget. We break from the "forgotten" async loops
                //in AcceptClientsAsync using a CancellationToken from `cts`
                await AcceptClientsAsync(listener, cts.Token);
                //Thread.Sleep(60000); //block here to hold open the server
            }
            finally
            {
                cts.Cancel();
                listener.Stop();
            }
        }

        async Task AcceptClientsAsync(TcpListener listener, CancellationToken ct)
        {
            var clientCounter = 0;
            while (!ct.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync()
                    .ConfigureAwait(false);
                //连接成功 连接provider端
                clientCounter++;
                //once again, just fire and forget, and use the CancellationToken
                //to signal to the "forgotten" async invocation.
                EchoAsync(client, new TcpClient(), clientCounter, ct);
            }

        }

        async Task EchoAsync(TcpClient client, TcpClient proxyClient,
            int clientIndex,
            CancellationToken ct)
        {
            Console.WriteLine("New client ({0}) connected", clientIndex);
            //转发buffer
            proxyClient.Connect("172.20.66.84", 80);
            //using (client)
            //using (proxyClient)
            //{

            var providerStream = proxyClient.GetStream();
            var consumerStream = client.GetStream();
            Task taskC2PLooping = C2PLooping(ct, consumerStream, providerStream);
            Task taskP2CLooping = P2CLooping(ct, consumerStream, providerStream);


            //循环接受A并写入C

            //}
            var comletedTask = await Task.WhenAny(taskC2PLooping, taskP2CLooping);
            //comletedTask.
            Console.WriteLine("Client ({0}) disconnected", clientIndex);
        }

        private async Task C2PLooping(CancellationToken ct, NetworkStream consumerStream, NetworkStream providerStream)
        {

            var buf = new byte[4096];

            //循环接收C并且写入A
            while (!ct.IsCancellationRequested)
            {
                //15秒没有心跳数据，则关闭连接释放资源
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15));
                var amountReadTask = consumerStream.ReadAsync(buf, 0, buf.Length, ct);
                //var providerReadTask = stream.ReadAsync(providBuf, 0, providBuf.Length, ct);
                //15秒到了或者读取到了内容则进行<\X/>下一个时间片
                var completedTask = await Task.WhenAny(timeoutTask, amountReadTask);

                // 非windowsform不需要 .ConfigureAwait(false);
                if (completedTask == timeoutTask)
                {
                   // var msg = Encoding.ASCII.GetBytes("consumer timed out");
                    Console.WriteLine("consumer timed out");
                   // await consumerStream.WriteAsync(msg, 0, msg.Length);
                    break;
                }

                //在接收到信息之后可以立即发送一些消息给客户端。
                //
                //now we know that the amountTask is complete so
                //we can ask for its Result without blocking
                var amountRead = amountReadTask.Result;
                if (amountRead == 0) break; //end of stream.

                //转发
                await providerStream.WriteAsync(buf, 0, amountRead, ct);
            }
        }

        private async Task P2CLooping(CancellationToken ct, NetworkStream consumerStream, NetworkStream providerStream)
        {

            var buf = new byte[4096];

            //循环接收C并且写入A
            while (!ct.IsCancellationRequested)
            {
                //15秒没有心跳数据，则关闭连接释放资源
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15));
                var amountReadTask = providerStream.ReadAsync(buf, 0, buf.Length, ct);
                //var providerReadTask = stream.ReadAsync(providBuf, 0, providBuf.Length, ct);
                //15秒到了或者读取到了内容则进行<\X/>下一个时间片
                var completedTask = await Task.WhenAny(timeoutTask, amountReadTask);

                // 非windowsform不需要 .ConfigureAwait(false);
                if (completedTask == timeoutTask)
                {
                    //var msg = Encoding.ASCII.GetBytes("provider timed out");
                    Console.WriteLine("provider timed out");
                    //await consumerStream.WriteAsync(msg, 0, msg.Length);
                    break;
                }

                //在接收到信息之后可以立即发送一些消息给客户端。
                //
                //now we know that the amountTask is complete so
                //we can ask for its Result without blocking
                var amountRead = amountReadTask.Result;
                if (amountRead == 0) break; //end of stream.

                //转发
                await consumerStream.WriteAsync(buf, 0, amountRead, ct);
            }
        }
    }
}