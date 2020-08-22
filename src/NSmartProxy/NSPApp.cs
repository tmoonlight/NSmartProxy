using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NSmartProxy.Data;
using NSmartProxy.Infrastructure.Extensions;
using NSmartProxy.Shared;

namespace NSmartProxy
{


    public class NSPApp
    {
        public int AppId;
        public string Description;//app名字，用来标识app
        public int ClientId;
        public int ConsumePort;
        //public TcpListener Listener;
        public CancellationTokenSource CancelListenSource;
        public PeekableBufferBlock<TcpClient> TcpClientBlocks; //反向连接的阻塞队列,一般只有一个元素
        public List<TcpTunnel> Tunnels;          //正在使用的隧道
        public List<TcpClient> ReverseClients;  //反向连接的socket
        public Protocol AppProtocol; //协议0 tcp 1 http
        public string Host;//主机头
        //public X509Certificate2 Certificate;//证书

        private bool _closed = false;
        public bool IsClosed => _closed;
        public bool IsCompress = false;//代表是否使用snappy压缩

        public NSPApp()
        {
            CancelListenSource = new CancellationTokenSource();
            TcpClientBlocks = new PeekableBufferBlock<TcpClient>();
            Tunnels = new List<TcpTunnel>();
            ReverseClients = new List<TcpClient>();
            //HttpApps = new Dictionary<string, NSPApp>();
        }

        /// <summary>
        /// 给app分配的内存压入tcpclient缓存块
        /// </summary>
        /// <param name="incomeClient"></param>
        /// <returns></returns>
        public void PushInComeClient(TcpClient incomeClient)
        {
            TcpClientBlocks.Post(incomeClient);
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
        public int Close(bool isForceClose = false)
        {
            if (!_closed)
            {
                int closedCount = 0;
                try
                {
                    foreach (var t in Tunnels)
                    {
                        //TODO 3调试用
                        //Console.WriteLine("XXX");
                        ////Console.WriteLine(t.ConsumerClient?.Client.LocalEndPoint);
                        //Console.WriteLine("XXX");
                        if (t.ClientServerClient != null && t.ClientServerClient.Connected)
                        {
                            t.ClientServerClient.Close();
                        }


                        if (t.ConsumerClient != null && t.ConsumerClient.Connected)
                        {
                            //关闭会直接出timewat
                            t.ConsumerClient.LingerState.Enabled = true;
                            t.ConsumerClient.LingerState.LingerTime = 0;
                            t.ConsumerClient.NoDelay = true;
                            //t.ConsumerClient.Client.Shutdown(SocketShutdown.Both);
                            t.ConsumerClient.Close();
                        }

                        closedCount++;
                    }

                    //关闭循环和当前的侦听
                    CancelListenSource?.Cancel();
                    //Listener?.Stop();//TODO 3 逻辑错误！这个侦听可能还共享给了其他的app
                    //弹出TcpClientBlocks
                    while (TcpClientBlocks.Count > 0)
                    {
                        TcpClient tcpClient = TcpClientBlocks.Receive();
                        if (isForceClose)
                        {
                            try
                            {
                                tcpClient.GetStream().Write(new byte[] { (byte)ControlMethod.ForceClose }, 0, 1);
                            }
                            catch (Exception ex)
                            {
                                Server.Logger.Debug("尝试抢登强制关闭失败：" + ex.ToString());
                            }
                        }
                        tcpClient.Close();
                    }
                    _closed = true;
                    return closedCount;
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