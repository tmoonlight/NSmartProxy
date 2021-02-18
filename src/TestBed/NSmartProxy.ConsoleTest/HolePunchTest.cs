using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace NSmartProxy.ConsoleTest
{
    //1v1匹配
    class WaitPeer
    {
        public IPEndPoint InternalAddr { get; private set; }
        public IPEndPoint ExternalAddr { get; private set; }
        public DateTime RefreshTime { get; private set; }

        public void Refresh()
        {
            RefreshTime = DateTime.Now;
        }

        public WaitPeer(IPEndPoint internalAddr, IPEndPoint externalAddr)
        {
            Refresh();
            InternalAddr = internalAddr;
            ExternalAddr = externalAddr;
        }
    }

    class HolePunchServerTest : INatPunchListener
    {
        private const int ServerPort = 12021;
        private const string ConnectionKey = "test_key";
        private static readonly TimeSpan KickTime = new TimeSpan(0, 0, 60);

        //等待队列（匹配）搞个list，代表p2p主机端
        private readonly Dictionary<string, WaitPeer> _hostingPeers = new Dictionary<string, WaitPeer>();
        private readonly List<string> _peersToRemove = new List<string>();
        private NetManager _puncher;
        //private NetManager _c1;
        //private NetManager _c2;

        //用这个token标识服务器和客户端？
        void INatPunchListener.OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token,byte HostOrClient)
        {
            WaitPeer wpeer;
            if (_hostingPeers.TryGetValue(token, out wpeer))
            {
                if (wpeer.InternalAddr.Equals(localEndPoint) &&
                    wpeer.ExternalAddr.Equals(remoteEndPoint))
                {
                    wpeer.Refresh();
                    return;
                }

                Console.WriteLine($"Wait peer found, sending introduction...hostOrClient {HostOrClient}");

                //found in list - introduce client and host to eachother
                //修改，将主机介绍给服务器
                Console.WriteLine(
                    "host - i({0}) e({1})\nclient - i({2}) e({3})",
                    wpeer.InternalAddr,
                    wpeer.ExternalAddr,
                    localEndPoint,
                    remoteEndPoint);

                //1对1 打通隧道
                _puncher.NatPunchModule.NatIntroduce(
                    wpeer.InternalAddr, // host internal
                    wpeer.ExternalAddr, // host external
                    localEndPoint, // client internal
                    remoteEndPoint, // client external
                    token // request token
                    );

                //Clear dictionary
                //_waitingPeers.Remove(token);
            }
            else
            {
                Console.WriteLine("Wait peer created. i({0}) e({1})", localEndPoint, remoteEndPoint);
                _hostingPeers[token] = new WaitPeer(localEndPoint, remoteEndPoint);
            }
        }

        void INatPunchListener.OnNatIntroductionSuccess(IPEndPoint targetEndPoint, string token)
        {
            //Ignore we are server
        }

        public void Run()
        {
            Console.WriteLine("=== HolePunch Test ===");

            EventBasedNetListener netListener = new EventBasedNetListener();
            //EventBasedNatPunchListener natPunchListener1 = new EventBasedNatPunchListener();
            //EventBasedNatPunchListener natPunchListener2 = new EventBasedNatPunchListener();

            netListener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("PeerConnected: " + peer.EndPoint.ToString());
            };

            netListener.NetworkReceiveEvent += NetListener_NetworkReceiveEvent;
            netListener.ConnectionRequestEvent += request =>
            {
                request.AcceptIfKey(ConnectionKey);
            };

            netListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                Console.WriteLine("PeerDisconnected: " + disconnectInfo.Reason);
                if (disconnectInfo.AdditionalData.AvailableBytes > 0)
                {
                    Console.WriteLine("Disconnect data: " + disconnectInfo.AdditionalData.GetInt());
                }
            };

            //natPunchListener1.NatIntroductionSuccess += (point, token) =>
            //{
            //    var peer = _c1.Connect(point, ConnectionKey);
            //    Console.WriteLine("Success C1. Connecting to C2: {0}, connection created: {1}", point, peer != null);
            //    peer.Send(Encoding.ASCII.GetBytes("hello1"), DeliveryMethod.ReliableOrdered);
            //};

            //natPunchListener2.NatIntroductionSuccess += (point, token) =>
            //{
            //    var peer = _c2.Connect(point, ConnectionKey);
            //    Console.WriteLine("Success C2. Connecting to C1: {0}, connection created: {1}", point, peer != null);
            //    peer.Send(Encoding.ASCII.GetBytes("hello2"), DeliveryMethod.ReliableOrdered);
            //};

            //_c1 = new NetManager(netListener);
            //_c1.NatPunchEnabled = true;
            //_c1.NatPunchModule.Init(natPunchListener1);
            //_c1.Start();

            //_c2 = new NetManager(netListener);
            //_c2.NatPunchEnabled = true;
            //_c2.NatPunchModule.Init(natPunchListener2);
            //_c2.Start();

            _puncher = new NetManager(netListener);
            _puncher.Start(ServerPort);
            _puncher.NatPunchEnabled = true;
            _puncher.NatPunchModule.Init(this);

            //_c1.NatPunchModule.SendNatIntroduceRequest(NetUtils.MakeEndPoint("::1", ServerPort), "token2");
            //_c2.NatPunchModule.SendNatIntroduceRequest(NetUtils.MakeEndPoint("::1", ServerPort), "token2");
            //_c1.NatPunchModule.SendNatIntroduceRequest(NetUtils.MakeEndPoint("::1", ServerPort), "token2");
            //_c1.NatPunchModule.SendNatIntroduceRequest(NetUtils.MakeEndPoint("::1", ServerPort), "token2");
            // _c2.NatPunchModule.SendNatIntroduceRequest(NetUtils.MakeEndPoint("::1", ServerPort), "token2");

            // keep going until ESCAPE is pressed
            Console.WriteLine("Press ESC to quit");

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Escape)
                    {
                        break;
                    }
                //    if (key == ConsoleKey.A)
                //    {
                //        Console.WriteLine("C1 stopped");
                //        _c1.DisconnectPeer(_c1.FirstPeer, new byte[] { 1, 2, 3, 4 });
                //        _c1.Stop();
                //    }

                //    if (key == ConsoleKey.B)
                //    {
                //        var peers = _c1.GetPeers();
                //        peers[0].Send(Encoding.ASCII.GetBytes("hello"), DeliveryMethod.ReliableOrdered);
                //    }
                }

                DateTime nowTime = DateTime.Now;

                _puncher.NatPunchModule.PollEvents();

                //check old peers
                //清空无心跳的peer（改为：清除其他的peer）
                //foreach (var waitPeer in _waitingPeers)
                //{
                //    if (nowTime - waitPeer.Value.RefreshTime > KickTime)
                //    {
                //        _peersToRemove.Add(waitPeer.Key);
                //    }
                //}

                ////remove
                //for (int i = 0; i < _peersToRemove.Count; i++)
                //{
                //    Console.WriteLine("Kicking peer: " + _peersToRemove[i]);
                //    _waitingPeers.Remove(_peersToRemove[i]);
                //}
                //_peersToRemove.Clear();

                Thread.Sleep(10);
            }

            //_c1.Stop();
            //_c2.Stop();
            _puncher.Stop();
        }

        private void NetListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            Console.WriteLine("reiceived");
        }
    }
}
