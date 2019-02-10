using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSmartProxy
{
    public class ClientConnectionManager
    {
        private ClientConnectionManager()
        {
            Console.WriteLine("ClientManager initialized");
            Task.Run(ListenServiceClient);
        }

        private async Task ListenServiceClient()
        {
            //侦听，并且构造连接池
            //throw new NotImplementedException();
            TcpListener listenter = new TcpListener(IPAddress.Any, Server.CLIENT_SERVER_PORT);
            listenter.Start(1000);
            while (1 == 1)
            {
                TcpClient incomeClient = await listenter.AcceptTcpClientAsync();
                Console.WriteLine("已建立一个空连接");
                AddClient(incomeClient);
            }

        }

        //可能要改成字典
        private Queue<TcpClient> ServiceClientQueue = new Queue<TcpClient>();
        private static ClientConnectionManager Instance = new Lazy<ClientConnectionManager>(() => new ClientConnectionManager()).Value;
        //Queue<TcpClient> IdleClientsQueue = new Queue<TcpClient>();
        public void AddClient(TcpClient client)
        {
            ServiceClientQueue.Enqueue(client);
        }

        public static ClientConnectionManager GetInstance()
        {
            return Instance;
        }

        public TcpClient GetClient()
        {
            return ServiceClientQueue.Dequeue();
        }
    }
}
