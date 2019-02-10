using NSmartProxy.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSmartProxy
{
    public class ClientGroupEventArgs : EventArgs
    {
        public IEnumerable<TcpClient> NewClients;
    }

    public class ServerConnnectionManager
    {


        private int MAX_CONNECT_SIZE = 10;
        private ServerConnnectionManager()
        {
            Console.WriteLine("ServerConnnectionManager initialized.");
            Task.Run(PollingToProvider);
        }

        public static event EventHandler ClientGroupConnected;
        private async Task PollingToProvider()
        {
            int hungryNumber = MAX_CONNECT_SIZE / 2;
            //侦听，并且构造连接池
            //throw new NotImplementedException();
            //int currentClientCount = ServiceClientQueue.Count;

            while (1 == 1)
            {
                int activeClientCount = 0;
                foreach (TcpClient c in ServiceClientList)
                {
                    if (c.Connected) activeClientCount++;
                }
                if (activeClientCount < hungryNumber)
                {
                    Console.WriteLine("连接已接近饥饿值，扩充连接池");

                    var clientList = new List<TcpClient>();
                    //补齐
                    for (int i = activeClientCount; i < MAX_CONNECT_SIZE; i++)
                    {
                        TcpClient client = new TcpClient();
                        client.Connect(ClientRouter.PROVIDER_ADDRESS, ClientRouter.PROVIDER_ADDRESS_PORT);
                        ServiceClientList.Add(client);
                        clientList.Add(client);
                    }
                    ClientGroupConnected(this, new ClientGroupEventArgs() { NewClients = clientList });
                }
                await Task.Delay(2000);
                //currentClientCount = ServiceClientQueue.Count;
            }
        }

        //可能要改成字典
        private List<TcpClient> ServiceClientList = new List<TcpClient>();
        private static ServerConnnectionManager Instance = new Lazy<ServerConnnectionManager>(() => new ServerConnnectionManager()).Value;
        //Queue<TcpClient> IdleClientsQueue = new Queue<TcpClient>();
        public void AddClient(TcpClient client)
        {
            ServiceClientList.Add(client);
        }

        public static ServerConnnectionManager GetInstance()
        {
            return Instance;
        }

        public TcpClient RemoveClient(TcpClient client)
        {
            if (ServiceClientList.Remove(client))

                return client;
            else
            {
                throw new Exception("无此client");
            }
        }
    }
}
