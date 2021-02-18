using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Text;
using System.Threading;

namespace NSmartProxy.P2PClient46
{
    class Program
    {
        private const int ServerPort = 12021;
        private const string ConnectionKey = "test_key";
        private const byte hostByte = 0;
        private const byte clientByte = 1;
        static void Main(string[] args)
        {

            EventBasedNetListener netListener = new EventBasedNetListener();
            EventBasedNatPunchListener netPunchListener = new EventBasedNatPunchListener();

            NetManager client = new NetManager(netListener);
            NetPeer peer = null;
            netPunchListener.NatIntroductionRequest += NetPunchListener_NatIntroductionRequest1;

            //收到这个消息说明通道已经打通
            netPunchListener.NatIntroductionSuccess += (point, token) =>
            {
                peer = client.Connect(point, ConnectionKey);//peer必须马上用 否则就没了？
                Console.WriteLine("Success . Connecting to : {0}, connection created: {1}", point, peer != null);
                //peer.Send(Encoding.UTF8.GetBytes("hello1"), DeliveryMethod.ReliableOrdered);
            };


            netListener.NetworkReceiveEvent += NetListener_NetworkReceiveEvent;
            netListener.PeerConnectedEvent += NetListener_PeerConnectedEvent;

            //netListener.PeerConnectedEvent += peer =>
            //{
            //    Console.WriteLine("PeerConnected: " + peer.EndPoint.ToString());
            //};

            //netListener.NetworkReceiveEvent += NetListener_NetworkReceiveEvent;
            netListener.ConnectionRequestEvent += request =>
            {
                request.AcceptIfKey(ConnectionKey);//关键方法
                Console.WriteLine("acceptifkey: connectionkey");
            };

            netListener.PeerDisconnectedEvent += (p, disconnectInfo) =>
            {
                Console.WriteLine("PeerDisconnected: " + disconnectInfo.Reason);
                if (disconnectInfo.AdditionalData.AvailableBytes > 0)
                {
                    Console.WriteLine("Disconnect data: " + disconnectInfo.AdditionalData.GetInt());
                }
            };

            client.NatPunchEnabled = true;
            client.NatPunchModule.Init(netPunchListener);
            //client.LocalPort;
            client.Start();
            client.NatPunchModule.SendNatIntroduceRequest(NetUtils.MakeEndPoint("2017studio.imwork.net", ServerPort), "token2", hostByte);
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Escape)
                    {
                        break;
                    }
                    if (key == ConsoleKey.A)
                    {
                        //NetDataWriter ndw = new NetDataWriter();

                        //peer.Send()
                        //peer.Send();
                        NetDataWriter ndw = new NetDataWriter();
                        ndw.Put("hello,im" + netListener.GetHashCode());
                        ndw.Put(new byte[2000]);
                        peer.Send(ndw, DeliveryMethod.ReliableOrdered);
                        //client.SendToAll(Encoding.ASCII.GetBytes("hello,im" + netListener.GetHashCode()), DeliveryMethod.ReliableOrdered);
                    }
                }
                client.NatPunchModule.PollEvents();
                client.PollEvents();
                Thread.Sleep(10);
            }
            Console.WriteLine("Hello World!");
        }

        private static void NetListener_PeerConnectedEvent(NetPeer peer)
        {
            int x = 0;
        }

        private static void NetPunchListener_NatIntroductionRequest1(System.Net.IPEndPoint localEndPoint, System.Net.IPEndPoint remoteEndPoint, string token, byte hostOrClient)
        {
            Console.WriteLine("introrequest received:" + token + ",hostOrClient" + hostOrClient);
        }



        private static void NetListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            Console.WriteLine("message received:" + reader.GetString());
        }
    }
}
