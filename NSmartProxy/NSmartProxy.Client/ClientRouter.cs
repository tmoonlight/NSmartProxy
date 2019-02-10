using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSmartProxy.Client
{
    public class ClientRouter
    {
        public const string TARGET_SERVICE_ADDRESS = "192.168.1.2";
        public const int TARGET_SERVICE_ADDRESS_PORT = 80;
        public const string PROVIDER_ADDRESS = "192.168.1.2";
        public const int PROVIDER_ADDRESS_PORT = 9973;
        CancellationTokenSource CANCELTOKEN = new CancellationTokenSource();
        CancellationTokenSource TRANSFERING_TOKEN = new CancellationTokenSource();
        ServerConnnectionManager ConnnectionManager;
        //连接server的client，始终存在
        //Queue<TcpClient> providerClients = new Queue<TcpClient>();
        //TcpClient providerClient = new TcpClient();
        public async Task ConnectToProvider()
        {


            ServerConnnectionManager.ClientGroupConnected += ServerConnnectionManager_ClientGroupConnected;
            ConnnectionManager = ServerConnnectionManager.GetInstance();
        }

        private void ServerConnnectionManager_ClientGroupConnected(object sender, EventArgs e)
        {
            var args = (ClientGroupEventArgs)e;
            foreach (TcpClient providerClient in args.NewClients)
            {

                Console.WriteLine("开启连接");
                OpenTrasferation(providerClient);
            }

        }

        private async Task OpenTrasferation(TcpClient providerClient)
        {
            byte[] buffer = new byte[4096];
            var providerClientStream = providerClient.GetStream();
            int readByteCount = await providerClientStream.ReadAsync(buffer);
            ConnnectionManager.RemoveClient(providerClient);
            Console.WriteLine("接受到首条信息");
            TcpClient toTargetServer = new TcpClient();
            toTargetServer.Connect(TARGET_SERVICE_ADDRESS, TARGET_SERVICE_ADDRESS_PORT);
            NetworkStream targetServerStream = toTargetServer.GetStream();
            targetServerStream.Write(buffer, 0, readByteCount);
            await TcpTransferAsync(providerClientStream, targetServerStream);
            //关闭连接
            providerClient.Close();
            Console.WriteLine("关闭一条连接");
        }


        private async Task TcpTransferAsync(NetworkStream providerStream, NetworkStream targetServceStream)
        {
            //while (!CANCELTOKEN.Token.IsCancellationRequested)
            //{
                Console.WriteLine("Looping start.");
                //创建相互转发流
                var taskT2PLooping = ToStaticTransfer(TRANSFERING_TOKEN.Token, targetServceStream, providerStream, "T2P");
                var taskP2TLooping = StreamTransfer(TRANSFERING_TOKEN.Token, providerStream, targetServceStream, "P2T");


                //循环接受A并写入C
                var comletedTask = await Task.WhenAny(taskT2PLooping, taskP2TLooping);
                //comletedTask.
                Console.WriteLine(comletedTask.Result + "传输关闭，重新读取字节");
            //    TRANSFERING_TOKEN.Cancel();
            //}
        }


        /// <summary>
        /// 流间传输
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="fromStream"></param>
        /// <param name="toStream"></param>
        /// <param name="beforeTransfer"></param>
        /// <returns></returns>
        private async Task<string> StreamTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream, string signal, Func<byte[], Task<bool>> beforeTransfer = null)
        {
            await fromStream.CopyToAsync(toStream, ct);
            return signal;
        }

        /// <summary>
        /// 流间传输
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="fromStream"></param>
        /// <param name="toStream"></param>
        /// <param name="beforeTransfer"></param>
        /// <returns></returns>
        private async Task<string> ToStaticTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream, string signal, Func<byte[], Task<bool>> beforeTransfer = null)
        {

            await fromStream.CopyToAsync(toStream, ct);
            return signal;
        }

        private void SendZero(int port)
        {
            TcpClient tc = new TcpClient();
            tc.Connect("127.0.0.1", port);
            tc.Client.Send(new byte[] { 0 });
        }
    }

}
