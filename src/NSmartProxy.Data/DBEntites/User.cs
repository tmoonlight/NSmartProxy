using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

namespace NSmartProxy.Data.DBEntities
{
    public class User
    {
        public string userId;
        public string userPwd;
        public string userName;
        public string regTime;
        public string isAdmin;
        public string isAnonymous; //1表示是 0表示否，匿名用户
    }
}
