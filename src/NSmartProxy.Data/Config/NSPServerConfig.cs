using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Data.Config
{
    public class NSPServerConfig
    {
        public int ClientServicePort = 19974;   //服务端代理转发端口
        public int ConfigServicePort = 12308;  //服务端配置通讯端口
        public int WebAPIPort = 12309;    //远端管理端口

        public ServerBoundConfig BoundConfig = new ServerBoundConfig();
    }
}
