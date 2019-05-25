using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Data.Entity
{
    public class User
    {
        public string userId;
        public string userPwd;
        public string userName;
        public string regTime;
        public string isAdmin;
        public string arrangedPorts;//锁定的端口，不会被其他任何用户使用
    }
}
