using NSmartProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPTester.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //int port = int.Parse(args[0]);
            int port = 5900;
            Console.WriteLine("start listen " + port.ToString());
            TcpListener tcpListener = TcpListener.Create(port);
            tcpListener.Start();
            TcpClient client = null;
            CancellationTokenSource cts = null;
            var thread = new Thread(() =>
            {
                while (true)
                {
                    client = tcpListener.AcceptTcpClient();
                    client.SetKeepAlive(out _);

                    //Task.Run(() =>
                    //{
                    //    var stream = client.GetStream();
                    //    while (true)
                    //    {
                    //        byte[] bytes = new byte[4096];
                    //        try
                    //        {
                    //            cts = new CancellationTokenSource();
                    //            Task<int> result = stream.ReadAsync(bytes, 0, bytes.Length, cts.Token);
                    //            result.Wait(cts.Token);
                    //            var length = result.Result;
                    //            Console.WriteLine("continue...");
                    //            Console.WriteLine(ASCIIEncoding.ASCII.GetString(bytes, 0, length).Trim());
                    //            string retMessage = "received++" + ASCIIEncoding.ASCII.GetString(bytes, 0, length).Trim() + "+++";
                    //            byte[] retMessageBytes = ASCIIEncoding.ASCII.GetBytes(retMessage);
                    //            stream.Write(retMessageBytes, 0, retMessageBytes.Length);
                    //        }
                    //        catch
                    //        {
                    //            break;
                    //        }

                    //    }

                    //});
                }

            });

            thread.Start();
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "stop")
                {
                    //client.Close();
                    //cts.Cancel(true);
                    TcpClient tc = new TcpClient();
                    tc.Connect("127.0.0.1", 12306);
                    tc.Client.Send(new byte[] { 0 });
                }
            }



        }

        public static void test111()
        {
        }
    }
}
