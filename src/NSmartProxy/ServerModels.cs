using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NSmartProxy
{
    public struct ClientIDAppID
    {
        public int ClientID;
        public int AppID;
    }

    public class TcpTunnel
    {
        public TcpClient ConsumerClient;
        public TcpClient ClientServerClient;
    }

    public class AppModel
    {
        public ClientIDAppID ClientIdAppId;
        public List<TcpTunnel> Tunnels;          //正在使用的隧道
        public List<TcpClient> ReverseClients;  //反向连接的socket
    }

    public class AppChangedEventArgs : EventArgs
    {
        public ClientIDAppID App;
    }
}
