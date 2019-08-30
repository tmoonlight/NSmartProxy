using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NSmartProxy.Shared;

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
        public List<TcpTunnel> Tunnels;          //正在使用的隧道
        public List<TcpClient> ReverseClients;  //反向连接的socket
        public int AppProtocol; //协议0 tcp 1 http
        public string Host;//主机头

        private bool _closed = false;

        public NSPApp()
        {
            CancelListenSource = new CancellationTokenSource();
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
            TcpClient tcpClient = null;
            var receiveTask = Task.Run(async () => { tcpClient = await TcpClientBlocks.ReceiveAsync(); });
            await Task.WhenAny(receiveTask, Task.Delay(Global.DefaultPopClientTimeout));
            //if (!isReceived) return -1;
            return tcpClient;
            //return await TcpClientBlocks.ReceiveAsync();
        }

        /// <summary>
        /// 关闭整个App
        /// </summary>
        public int Close()
        {
            if (!_closed)
            {
                int ClosedCount = 0;
                try
                {
                    Tunnels.ForEach((t) =>
                    {
                        t.ClientServerClient?.Close();
                        t.ConsumerClient?.Close();
                        ClosedCount++;
                    });
                    //关闭循环和当前的侦听
                    CancelListenSource?.Cancel();
                    Listener?.Stop();
                    //弹出TcpClientBlocks
                    while (TcpClientBlocks.Count > 0)
                    {
                        TcpClientBlocks.Receive().Close();
                    }
                    _closed = true;
                    return ClosedCount;
                }
                catch (Exception ex)
                {
                    Server.Logger.Debug($"关闭app({ClientId}-{AppId})失败:{ex}");
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }


    }
}