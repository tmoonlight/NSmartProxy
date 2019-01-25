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
        private TcpClient ProviderClient;
        public async Task Start()
        {
            //privider初始化
            ProviderClient = new TcpClient();


            //listenter初始化
            CancellationTokenSource cts = new CancellationTokenSource();
            TcpListener listener = new TcpListener(IPAddress.Any, 6666);
            try
            {
                listener.Start();
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
                EchoAsync(client, clientCounter, ct);
            }

        }

        async Task EchoAsync(TcpClient client,
            int clientIndex,
            CancellationToken ct)
        {
            Console.WriteLine("New client ({0}) connected", clientIndex);
            using (client)
            {
                var buf = new byte[4096];
                var stream = client.GetStream();
                //接收文本
                while (!ct.IsCancellationRequested)
                {
                    //under some circumstances, it's not possible to detect
                    //a client disconnecting if there's no data being sent
                    //so it's a good idea to give them a timeout to ensure that 
                    //we clean them up.
                    //15秒没有心跳数据，则关闭连接释放资源
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15));
                    var amountReadTask = stream.ReadAsync(buf, 0, buf.Length, ct);
                    //15秒到了或者读取到了内容则进行<\X/>下一个时间片
                    var completedTask = await Task.WhenAny(timeoutTask, amountReadTask)
                        .ConfigureAwait(false);
                    if (completedTask == timeoutTask)
                    {
                        var msg = Encoding.ASCII.GetBytes("Client timed out");
                        await stream.WriteAsync(msg, 0, msg.Length);
                        break;
                    }

                    //在接收到信息之后可以立即发送一些消息给客户端而不阻塞现在的线程。
                    //
                    //now we know that the amountTask is complete so
                    //we can ask for its Result without blocking
                    var amountRead = amountReadTask.Result;
                    if (amountRead == 0) break; //end of stream.

                    //转发buffer
                    ProviderClient.Connect("172.20.66.84",80);
                    ProviderClient.GetStream().WriteAsync(buf, 0, amountRead, ct);
                    
                    //await stream.WriteAsync(buf, 0, amountRead, ct)
                    //    .ConfigureAwait(false);

                    //接收响应
                    //<\X/>下一个时间片

                }
            }

            Console.WriteLine("Client ({0}) disconnected", clientIndex);
        }
    }
}