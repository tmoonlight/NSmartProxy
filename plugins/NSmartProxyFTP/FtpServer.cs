//作者：Mcdull
//说明：FTP服务端类，负责监听端口并建立连接。
//此处单独封装一个类是为了和UI层分离，使得UI仅需通过捕获事件获取通知而无须关心逻辑
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace FtpServer
{
    class FtpServer
    {
        private TcpListener listener;
        private bool isStart;
        private Thread mainThread;
        private int maxConnect;

        private IPAddress LocalIP { get; set; }
        private int Port { get; set; }
        private int PasvPort { get; set; }
        public static IEnumerable<UserElement> Users { get; private set; }

        public FtpServer(int port, int pasv_port, int max_connect, IEnumerable<UserElement> users)
        {
            clients = new List<FtpClient>();
            // listener = new TcpListener(ip, port);
            listener = new TcpListener(IPAddress.Any, port);
            maxConnect = max_connect;
            // LocalIP = ip;
            LocalIP = IPAddress.Any;
            Port = port;
            PasvPort = pasv_port;
            Users = users;
        }

        public List<FtpClient> clients;
        public event Action<FtpClient> clientChange;

        public bool Start(IPAddress pasv_proxy_ip, int pasv_proxy_port)
        {
            if (!isStart)
            {
                mainThread = new Thread(p =>
                {
                    while (isStart)
                    {
                        if (clients.Count < maxConnect)
                        {
                            try
                            {
                                listener.Start();
                                Socket socket = listener.AcceptSocket();
                                FtpClient client = new FtpClient(socket, LocalIP, PasvPort, pasv_proxy_ip, pasv_proxy_port);
                                client.Quit += client_Quit;
                                client.Login += client_Login;
                                clients.Add(client);
                                onClientChange(client);
                                client.Start();
                            }
                            catch
                            {
                                isStart = false;
                                return;
                            }
                        }
                        else
                        {
                            listener.Stop();
                        }
                        Thread.Sleep(2000);
                    }
                });
                mainThread.IsBackground = true;   //设置为后台线程，进程关闭后线程强制关闭（前台线程则会导致线程结束后进程才能结束）
                mainThread.Start();
                isStart = true;
                return true;
            }
            return false;
        }

        public bool Stop()
        {
            if (isStart)
            {
                isStart = false;
                listener.Stop();
                mainThread.Abort();
                foreach (var c in clients)
                    c.Stop();
                clients.Clear();
                return true;
            }
            return false;
        }

        void client_Login(FtpClient client)
        {
            onClientChange(client);
        }

        void client_Quit(FtpClient client)
        {
            clients.Remove(client);
            onClientChange(client);
        }

        private void onClientChange(FtpClient client)
        {
            var temp = clientChange;
            if (temp != null)
                temp(client);
        }
    }
}
