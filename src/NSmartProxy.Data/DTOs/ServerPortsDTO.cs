using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Data
{
    public class ServerPortsDTO
    {
        public int ReversePort = 19974;   //服务端代理转发端口
        public int ConfigPort = 12308;  //服务端配置通讯端口
        public int WebAPIPort = 12309;    //远端管理端口
    }
}
