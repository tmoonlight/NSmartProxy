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
            tcpClient.Connect("127.0.0.1", 5944);
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
            int x = 0;
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

                byte[] allbBytes = ASCIIEncoding.ASCII.GetBytes(str + x);
                stream.WriteTimeout = 3000;
                stream.Write(allbBytes, 0, allbBytes.Length);
                x++;
                Console.ReadLine();
            }

        }
    }
}
