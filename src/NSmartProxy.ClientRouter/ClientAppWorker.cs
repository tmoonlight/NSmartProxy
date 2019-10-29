using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NSmartProxy.Client
{
    public class ClientAppWorker
    {
        //private bool isWorking = false;

        //public List<TcpClient> TcpClientGroup = new List<TcpClient>();
        public TcpClient Client;//TODO 还是需要把这里改成复数
        public int AppId;  //1~255
        public int Port;   //0~65535

        //public bool IsWorking { get => isWorking;}

        //public void StartWork()
        //{
        //    isWorking = true;
        //}
    }
}
