using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPTester.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(2000);
            for (int i = 0; i < 1; i++)
            {
                Thread.Sleep(100);
                Task.Run(() =>
                {

                    // int port = int.Parse(args[0]);
                    TcpClient tcpClient = new TcpClient();
                    //tcpClient.Connect("192.168.1.168", port);
                    tcpClient.Connect("127.0.0.1", 5900);
                    var stream = tcpClient.GetStream();
                    stream.Write(new byte[] { 1 }, 0, 1);
                    stream.Write(new byte[] { 1 }, 0, 1);
                    Console.WriteLine("连接数" + i.ToString());

                    //Task.Run(() =>
                    //{

                    //    byte[] buffer = new byte[4096];
                    //    while (true)
                    //    {
                    //        int readResultLength = stream.Read(buffer, 0, buffer.Length);
                    //        //if(readResultLength)
                    //        Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer, 0, readResultLength).Trim());
                    //    }
                    //});



                    //while (true)
                    //{
                    //    string str = "testmessage" + Thread.CurrentThread.ManagedThreadId.ToString();
                    //    if (str == "c")
                    //    {
                    //        tcpClient.Close();
                    //        break;
                    //    }

                    //    byte[] allbBytes = ASCIIEncoding.ASCII.GetBytes(str);
                    //    stream.Write(allbBytes, 0, allbBytes.Length);
                    //    Thread.Sleep(3000);
                    //}
                });

            }

            Console.Read();


        }
    }
}
