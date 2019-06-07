using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Data.DTOs
{
    public class UserDTO
    {
        public string userId;
        public string userPwd;
        public string userName;
        public string regTime;
        public string isAdmin;
        public string boundPorts;//锁定的端口，不会被其他任何用户使用
        public string isAnonymous; //1表示是 0表示否，匿名用户
        public string isBanned; //被断开
        public string isOnline; //正在连接中
    }
}
