using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSmartProxy.Client
{
    public class ClientRouter
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        public async Task StartToProvider()
        {
            //连接，获取provider端连接
            TcpClient tcpClient = new TcpClient();
            TcpClient targetServiceClient = new TcpClient();        //目标服务

            tcpClient.Connect("172.20.66.84", 9973);
            Console.WriteLine("Provider connected.");
            NetworkStream providerStream = tcpClient.GetStream();
            NetworkStream targetServceStream = null;
            var buf = new byte[4096];

            int firstResult = await providerStream.ReadAsync(buf, 0, buf.Length, cts.Token);

            //服务端主动断开连接时，会收到0字节，此时关闭连接
            if (firstResult == 0) return;

            //if (!targetServiceClient.Connected)
            //{
            targetServiceClient.Connect("172.20.66.84", 80);
            targetServceStream = targetServiceClient.GetStream();
            //}

            await targetServceStream.WriteAsync(buf, 0, buf.Length);
            //读取provider的tcp流
            //ps：需要每隔一分钟发送一次心跳包，否则服务会自动断开
            while (!cts.IsCancellationRequested)
            {
                int resultLength = await providerStream.ReadAsync(buf, 0, buf.Length, cts.Token);

                //服务端主动断开连接时，会收到0字节，此时关闭连接
                if (resultLength == 0) break;

                await targetServceStream.WriteAsync(buf, 0, buf.Length);
            }

            //接收到
           
            Task taskC2PLooping = P2SLooping(cts.Token, targetServceStream, providerStream);
            Task taskP2CLooping = S2PLooping(cts.Token, targetServceStream, providerStream);


            //循环接受A并写入C
            var comletedTask = await Task.WhenAny(taskC2PLooping, taskP2CLooping);
            //comletedTask.
            Console.WriteLine("Some Client disconnected");

        }



        private async Task P2SLooping(CancellationToken ct, NetworkStream targetServerStream, NetworkStream providerStream)
        {

            var buf = new byte[4096];
            while (!ct.IsCancellationRequested)
            {
                //15秒没有心跳数据，则关闭连接释放资源
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15));
                var amountReadTask = providerStream.ReadAsync(buf, 0, buf.Length, ct);
                var completedTask = await Task.WhenAny(timeoutTask, amountReadTask);

                // 非windowsform不需要 .ConfigureAwait(false);
                if (completedTask == timeoutTask)
                {
                    Console.WriteLine("provider timed out");
                    break;
                }

              
                var amountRead = amountReadTask.Result;
                if (amountRead == 0) break; //end of stream.

                //转发
                await targetServerStream.WriteAsync(buf, 0, amountRead, ct);
            }
        }


        private async Task S2PLooping(CancellationToken ct, NetworkStream targetServerStream, NetworkStream providerStream)
        {

            var buf = new byte[4096];

            //循环接收C并且写入A
            while (!ct.IsCancellationRequested)
            {
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15));
                var amountReadTask = targetServerStream.ReadAsync(buf, 0, buf.Length, ct);
                var completedTask = await Task.WhenAny(timeoutTask, amountReadTask);

                if (completedTask == timeoutTask)
                {
                    Console.WriteLine("consumer timed out");

                    break;
                }

                var amountRead = amountReadTask.Result;
                if (amountRead == 0) break; //end of stream.
                await providerStream.WriteAsync(buf, 0, amountRead, ct);
            }
        }
    }

}
