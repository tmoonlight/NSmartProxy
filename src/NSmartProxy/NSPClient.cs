using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace NSmartProxy
{
    public class NSPClient
    {
        public int ClientID;
        public DateTime LastUpdateTime;
        public TcpClient ConfigClient;      //配置用的socket


        public Dictionary<int, NSPApp> AppMap; //Appid->app

        public NSPClient()
        {
            AppMap = new Dictionary<int, NSPApp>();
        }

        /// <summary>
        /// 注册app并且返回appid（非线程安全）
        /// </summary>
        /// <returns></returns>
        public int RegisterNewApp()
        {
            //按顺序分配最大int
            int preAppId = 1;
            if (AppMap.Count > 0)
                preAppId = AppMap.Last().Key + 1;

            NSPApp app = this.AppMap[preAppId] = new NSPApp()
            {
                AppId = preAppId,
                ClientId = ClientID
            };

            return app.AppId;
        }

        /// <summary>
        /// 绑定主机头host到某个端口上，同一端口可以多次绑定
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        //public int BindHost(string host, int port)
        //{
        //    if (AppMap[port].HttpApps.Count == 0)
        //    {
        //        AppMap.
        //    }

        //    AppMap[port].HttpApps.
        //}

        public NSPApp GetApp(int appId)
        {
            return AppMap[appId];
        }

        //public int Close()
        //{
        //    //统计关闭的连接数
        //    int ClosedConnectionCount = 0;
        //    foreach (var AppKV in AppMap)
        //    {
        //        ClosedConnectionCount += AppKV.Value.Close();
        //    }

        //    return ClosedConnectionCount;
        //}
    }
}