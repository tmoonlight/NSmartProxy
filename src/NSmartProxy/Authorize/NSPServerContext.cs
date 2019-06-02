using System;
using System.Collections.Generic;
using System.Text;
using NSmartProxy.Data;

namespace NSmartProxy.Authorize
{
    public class NSPServerContext
    {
        public NSPClientCollection Clients = new NSPClientCollection();
        public Dictionary<int, NSPApp> PortAppMap = new Dictionary<int, NSPApp>(); //端口和app的映射关系
        public HashSet<string> TokenCaches; //服务端会话池，登陆后的会话都在这里，每天需要做定时清理
        private bool supportAnonymousLogin = true;

        public NSPServerContext()
        {
            TokenCaches = new HashSet<string>();
        }

        /// <summary>
        /// 支持客户端匿名登陆
        /// </summary>
        public bool SupportAnonymousLogin { get => supportAnonymousLogin; set => supportAnonymousLogin = value; }

        public void CloseAllSourceByClient(int clientId)
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
