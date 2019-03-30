using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NSmartProxy
{
    public class NSPApp
    {
        public int AppId;
        public int ClientId;
        public int ConsumePort;
        public BufferBlock<TcpClient> TcpClientBlocks; //反向连接的阻塞队列,一般只有一个元素

        // public ClientIDAppID ClientIdAppId;
        public List<TcpTunnel> Tunnels;          //正在使用的隧道
        public List<TcpClient> ReverseClients;  //反向连接的socket

        public NSPApp()
        {
            TcpClientBlocks = new BufferBlock<TcpClient>();
            Tunnels = new List<TcpTunnel>();
            ReverseClients = new List<TcpClient>();
        }

        /// <summary>
        /// 给app分配的内存压入tcpclient缓存块
        /// </summary>
        /// <param name="incomeClient"></param>
        /// <returns></returns>
        public bool PushInComeClient(TcpClient incomeClient)
        {
            return TcpClientBlocks.Post(incomeClient);
        }

        /// <summary>
        /// 弹出tcpclient缓存，如果队列为空，则阻塞,该方法异步
        /// </summary>
        /// <returns></returns>
        public async Task<TcpClient> PopClientAsync()
        {
            return await TcpClientBlocks.ReceiveAsync();
        }
    }
}