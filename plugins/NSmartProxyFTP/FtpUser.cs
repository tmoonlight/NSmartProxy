//作者：Mcdull
//说明：FTP账号类
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace FtpServer
{
    sealed class User
    {
        public bool isLogin { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string workingDir { get; set; }
    }

    sealed class UserElement
    {
        public string username
        {
            get; set;
        }
        public string password
        {
            get; set;
        }
        public string rootDir
        {
            get; set;
        }
    }
}
