using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NSmartProxy
{
    public class NSPApp
    {
        public int AppId;
        public int ClientId;
        public int ConsumePort;
        public TcpListener Listener;
        public CancellationTokenSource CancelListenSource;

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

        /// <summary>
        /// 关闭整个App
        /// </summary>
        public int Close()
        {
            int ClosedCount = 0;
            Tunnels.ForEach((t) =>
            {
                t.ClientServerClient.Close(); t.ConsumerClient.Close();
                ClosedCount++;
            });
            //关闭循环和当前的侦听
            CancelListenSource.Cancel();
            Listener.Stop();
            //弹出TcpClientBlocks
            while (TcpClientBlocks.Count > 0)
            {
                TcpClientBlocks.Receive().Close();
            }

            return ClosedCount;
        }


    }
}