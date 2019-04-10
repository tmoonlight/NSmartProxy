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
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect("124.116.157.31", 5944);
            //tcpClient.Connect("192.168.1.2", 12306);
            var stream = tcpClient.GetStream();

            Task.Run(() =>
            {

                byte[] buffer = new byte[4096];
                while (true)
                {
                    int readResultLength = stream.Read(buffer, 0, buffer.Length);
                    
                    //if(readResultLength)
                    Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer, 0, readResultLength).Trim());
                }
            });

            while (true)
            {
                string str = "testgo ";//Console.ReadLine();
                if (str == "c")
                {
                    tcpClient.Close();
                    tcpClient.Close();
                    tcpClient.Close();
                    break;
                }

                byte[] allbBytes = ASCIIEncoding.ASCII.GetBytes(str);
                stream.WriteTimeout = 3000;
                stream.Write(allbBytes, 0, allbBytes.Length);
                Thread.Sleep(2000);
            }

        }
    }
}
