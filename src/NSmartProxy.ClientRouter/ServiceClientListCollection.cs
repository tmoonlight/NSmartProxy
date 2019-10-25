using NSmartProxy.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Client
{
    public class ServiceClientListCollection : Dictionary<int, ClientAppWorker>
    {
        /// <summary>
        /// 通过appid查询服务节点
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public ClientAppWorker GetAppFromPort(int appid)
        {
            if (this.ContainsKey(appid))
            {
                return this[appid];
            }
            else
            {
                return null;
            }
        }
    }
}
