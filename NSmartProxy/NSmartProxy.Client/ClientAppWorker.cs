using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NSmartProxy
{
    public class ClientAppWorker
    {
        public List<TcpClient> TcpClientGroup = new List<TcpClient>();
        public int AppId;  //1~255
        public int Port;   //0~65535
    }
}
