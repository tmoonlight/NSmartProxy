using System;
using System.Collections.Generic;
using System.Text;
using NSmartProxy.Data;
using NSmartProxy.Data.Config;

namespace NSmartProxy.Authorize
{
    /// <summary>
    /// 存放服务端的状态，以供其他组件共享
    /// </summary>
    public class NSPServerContext
    {
        public NSPClientCollection Clients;
        public Dictionary<int, NSPApp> PortAppMap;//“地址:端口”和app的映射关系
        public NSPServerConfig ServerConfig;
        public HashSet<string> TokenCaches; //服务端会话池，登录后的会话都在这里，每天需要做定时清理
        public long TotalReceivedBytes; //TODO 统计进出数据 下行
        public long TotalSentBytes;//上行
        public long ConnectCount;//连接次数
        public long ClientConnectCount; //客户端连接次数

        private bool supportAnonymousLogin = true;

        public NSPServerContext()
        {
            TokenCaches = new HashSet<string>();
            Clients = new NSPClientCollection();
            PortAppMap = new Dictionary<int, NSPApp>();
            //ServerConfig = new NSPServerConfig();
        }

        /// <summary>
        /// 支持客户端匿名登录
        /// </summary>
        public bool SupportAnonymousLogin { get => supportAnonymousLogin; set => supportAnonymousLogin = value; }

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

        /// <summary>
        /// 上下文中删除特定客户端
        /// </summary>
        /// <param name="clientId"></param>
        public void CloseAllSourceByClient(int clientId, bool addToBanlist = false)
        {
            if (Clients.ContainsKey(clientId))
            {
                NSPClient client = Clients[clientId];
                string msg = "";
                foreach (var appKV in client.AppMap)
                {
                    int port = appKV.Value.ConsumePort;
                    //1.关闭，并移除AppMap中的App
                    PortAppMap[port].Close();
                    PortAppMap.Remove(port);
                    msg += appKV.Value.ConsumePort + " ";
                    //2.移除端口占用
                    NetworkUtil.ReleasePort(port);
                }

                //3.移除client
                try
                {
                    int closedClients = Clients.UnRegisterClient(client.ClientID);
                    Server.Logger.Info(msg + $"已移除， {client.ClientID} 中的 {closedClients}个传输已终止。");
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
    }
}
