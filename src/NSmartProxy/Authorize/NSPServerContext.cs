using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NSmartProxy.Data;
using NSmartProxy.Data.Config;
using NSmartProxy.Infrastructure;
using NSmartProxy.Infrastructure.Interfaces;

namespace NSmartProxy.Authorize
{
    /// <summary>
    /// 存放服务端的状态，以供其他组件共享
    /// </summary>
    public class NSPServerContext : IServerContext
    {
        public NSPClientCollection Clients;
        public Dictionary<int, NSPAppGroup> PortAppMap;//“地址:端口”和app的映射关系
        public Dictionary<int, NSPAppGroup> UDPPortAppMap;//UDP “地址:端口”和app的映射关系
        public NSPServerConfig ServerConfig;
        public HashSet<string> TokenCaches; //服务端会话池，登录后的会话都在这里，每天需要做定时清理
        public long TotalReceivedBytes; //TODO 统计进出数据 下行
        public long TotalSentBytes;//上行
        public long ConnectCount;//连接次数
        public long ClientConnectCount; //客户端连接次数
        public Dictionary<string, X509Certificate> PortCertMap;//host->证书字典        

        public NSPServerContext()
        {
            TokenCaches = new HashSet<string>();
            Clients = new NSPClientCollection();
            PortAppMap = new Dictionary<int, NSPAppGroup>();
            UDPPortAppMap = new Dictionary<int, NSPAppGroup>();
            PortCertMap = new Dictionary<string, X509Certificate>();
            //ServerConfig = new NSPServerConfig();
        }

        /// <summary>
        /// 支持客户端匿名登录
        /// </summary>        
        public bool SupportAnonymousLogin { get => ServerConfig.supportAnonymousLogin; set => ServerConfig.supportAnonymousLogin = value; }

        public string ServerConfigPath { get; set; }

        /// <summary>
        /// 同步配置和排出网络端口列表，保持他们的一致性
        /// </summary>
        public void UpdatePortMap() //重新添加
        {
            NetworkUtil.ClearAllUsedPorts();
            foreach (var (user, userbound) in ServerConfig.BoundConfig.UserPortBounds)
            {
                if (userbound.Bound != null & userbound.Bound.Count > 0)
                {
                    NetworkUtil.AddUsedPorts(userbound.Bound);
                }
            }
        }

        public void AddPortMap() //使得被用户绑定的端口无法被分配到
        {
            foreach (var (user, userbound) in ServerConfig.BoundConfig.UserPortBounds)
            {
                if (userbound.Bound != null & userbound.Bound.Count > 0)
                {
                    NetworkUtil.AddUsedPorts(userbound.Bound);
                }
            }
        }

        private object locker = new object();
        /// <summary>
        /// 上下文中删除特定客户端
        /// </summary>
        /// <param name="clientId"></param>
        public void CloseAllSourceByClient(int clientId, bool addToBanlist = false, bool isForceClose = false)
        {
            if (Clients.ContainsKey(clientId))
            {
                NSPClient client = Clients[clientId];
                string msg = "";
                lock (locker)
                {
                    foreach (var appKV in client.AppMap)
                    {
                        int port = appKV.Value.ConsumePort;
                        var appMap = appKV.Value.AppProtocol == Protocol.UDP ? UDPPortAppMap : PortAppMap;
                        //1.关闭，并移除AppMap中的App
                        if (!appMap.ContainsKey(port))
                        {
                            Server.Logger.Debug($"clientid:{clientId}不包含port:{port}");
                        }
                        else
                        {
                            //TODO 3 关闭时会造成连锁反应
                            //TODO 3 并且当nspgroup里的元素全都被关闭时，才remove掉这个节点
                            //PortAppMap[port]
                            //PortAppMap[port].CloseByHost();
                            var nspAppGroup = appMap[port];
                            foreach (var (host, app) in nspAppGroup)
                            {
                                if (app.ClientId == clientId)
                                {
                                    app.Close(isForceClose);
                                }
                            }

                            if (nspAppGroup.IsAllClosed())
                            {
                                Server.Logger.Info($"端口{port}所有app退出，侦听终止");
                                if (nspAppGroup.Listener != null)
                                {
                                    //如果port内所有的app全都移除，才关闭listener
                                    nspAppGroup.Listener.Server.NoDelay = true;
                                    nspAppGroup.Listener.Server.Close();
                                    nspAppGroup.Listener.Stop();
                                }

                                if (nspAppGroup.UdpClient != null)
                                {
                                    nspAppGroup.UdpClient.Close();
                                }

                                appMap.Remove(port);
                            }
                        }


                        msg += appKV.Value.ConsumePort + " ";
                        //2.移除端口占用
                        NetworkUtil.ReleasePort(port);
                    }
                }

                //3.移除client
                try
                {
                    Clients.UnRegisterClient(client.ClientID);
                    Server.Logger.Info(msg + $"已移除， {client.ClientID} 中的 传输已终止。");
                }
                catch (Exception ex)
                {
                    Server.Logger.Error($"CloseAllSourceByClient error:{ex.Message}", ex);
                }
            }
            else
            {
                Server.Logger.Debug($"无此id: {clientId},可能已关闭过");
            }
        }

        /// <summary>
        /// 通过配置初始化证书到证书缓存中
        /// </summary>
        public void InitCertificates()
        {
            foreach (var (port, path) in ServerConfig.CABoundConfig)
            {
                if (File.Exists(path))
                {
                    //从文件里加载证书
                    PortCertMap[port] = X509Certificate2.CreateFromCertFile(path);
                }
                else
                {
                    Server.Logger.Debug($"未加载位于{port}的证书。");
                }
            }

        }

        public void SaveConfigChanges()
        {
            if (string.IsNullOrEmpty(ServerConfigPath))
            {
                throw new Exception("配置路径ServerConfigPath为空。");
            }

            ServerConfig.SaveChanges(ServerConfigPath);
        }

    }
}
