using NSmartProxy.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using NSmartProxy.Authorize;
using NSmartProxy.Data;
using NSmartProxy.Data.DTO;
using NSmartProxy.Data.DTOs;
using NSmartProxy.Data.Entity;
using NSmartProxy.Database;
using NSmartProxy.Shared;

namespace NSmartProxy.Extension
{
    /// <summary>
    /// 这里存放API
    /// </summary>
    partial class HttpServer
    {

        //TODO XXXX
        #region  dashboard
        [Secure]
        [API]
        public ServerStatusDTO GetServerStatus()
        {
            ServerStatusDTO dto = new ServerStatusDTO
            {
                connectCount = ServerContext.ConnectCount,
                totalReceivedBytes = ServerContext.TotalReceivedBytes,
                totalSentBytes = ServerContext.TotalSentBytes
            };
            return dto;
        }

        #endregion

        #region log
        [Secure]
        [API]
        public string[] GetLogFileInfo(string lastLines)
        {
            int lastLinesInt = int.Parse(lastLines);
            string baseLogPath = "./log";
            DirectoryInfo dir = new DirectoryInfo(baseLogPath);
            FileInfo[] files = dir.GetFiles("*.log*");
            DateTime recentWrite = DateTime.MinValue;
            FileInfo recentFile = null;

            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > recentWrite)
                {
                    recentWrite = file.LastWriteTime;
                    recentFile = file;
                }
            }

            //文件会被独占
            using (var fs = new FileStream(recentFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                var sr = new StreamReader(fs);
                return sr.Tail(lastLinesInt);
            }
        }

        /// <summary>
        /// 返回一个未关闭的stream
        /// </summary>
        /// <param name="filekey"></param>
        /// <returns></returns>
        [Secure]
        [FileAPI]
        public FileDTO GetLogFile(string filekey)
        {
            string allowedSuffix = ".log";
            string suffix = Path.GetExtension(filekey);
            string fileName = Path.GetFileName(filekey);
            string fileFullPath = BASE_LOG_FILE_PATH + "/" + filekey;
            if (allowedSuffix == suffix)
            {
                var fs = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read,
                    FileShare.Delete | FileShare.ReadWrite);
                return new FileDTO()
                {
                    FileName = filekey,
                    FileStream = fs
                };
            }
            string msg = $"文件{filekey}无效";
            //通过日志文件名获取文件
            Server.Logger.Error(msg, new Exception(msg));
            return null;
        }

        [Secure]
        [API]
        public string[] GetLogFiles()
        {
            string baseLogPath = "./log";
            DirectoryInfo dir = new DirectoryInfo(baseLogPath);
            var files = dir.GetFiles("*.log*").OrderByDescending(s => s.CreationTime);
            return files.Select(obj => obj.Name).ToArray();
        }
        #endregion

        #region config

        //private const string C_OPEN_DASH_BOARD = nameof(C_OPEN_DASH_BOARD);
        //private const string C_OPEN_LOG_TRACK = nameof(C_OPEN_LOG_TRACK);
        private const string AllowAnonymousUser = nameof(AllowAnonymousUser);
        //API SetConfig
        //API GetConifgs
        [API]
        [Secure]
        public string SetConfig(string key, string value)
        {
            switch (key)
            {
                case AllowAnonymousUser:
                    ServerContext.SupportAnonymousLogin = (value == "1");
                    ; break;
                default: return "";
            }

            return "";
        }

        [API]
        [Secure]
        public string GetConfig(string key)
        {
            switch (key)
            {
                case AllowAnonymousUser:
                    return ServerContext.SupportAnonymousLogin ? "1" : "0";
                default: return "";
            }
        }

        #endregion

        #region login
        [FormAPI]
        public string Login(string username, string userpwd)
        {
            //1.校验
            dynamic user = Dbop.Get(username)?.ToDynamic();
            if (user == null)
            {
                return "Error: User not exist.Please <a href='javascript:history.go(-1)'>go backward</a>.";
            }


            if (user.userPwd != EncryptHelper.SHA256(userpwd))
            {
                return "Error: Wrong password.Please <a href='javascript:history.go(-1)'>go backward</a>.";
            }

            //2.给token
            string output = $"{username}|{DateTime.Now.ToString("yyyy-MM-dd")}";
            string token = EncryptHelper.AES_Encrypt(output);
            return string.Format(@"
<html>
<head><script>
document.cookie='NSPTK={0}; path=/;';
document.write('Redirecting...');
window.location.href='main.html';
</script>
</head>
</html>
            ", token);
        }

        /// <summary>
        /// 提供非web的登录方法
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userpwd"></param>
        /// <returns></returns>
        [API]
        public LoginFormClientResult LoginFromClient(string username, string userpwd)
        {
            User user = null;
            //匿名登录时創建一個用戶
            if (ServerContext.SupportAnonymousLogin && string.IsNullOrEmpty(username))
            {

                username = "temp_" + RandomHelper.NextString(12, false);
                userpwd = RandomHelper.NextString(20);
                user = new User
                {
                    userId = NSmartDbOperator.SUPER_VARIABLE_INDEX_ID,  //索引id
                    userName = username,
                    userPwd = EncryptHelper.SHA256(userpwd),
                    regTime = DateTime.Now.ToString(),
                    isAdmin = "0",
                    isAnonymous = "1"
                };
                //if (isAdmin == true) user.
                //1.增加用户
                Dbop.Insert(username, user.ToJsonString());

            }
            //1.校验
            user = Dbop.Get(username)?.ToObject<User>();
            if (user == null)
            {
                throw new Exception("error: user not exist.");
            }

            if (ServerContext.ServerConfig.BoundConfig.UsersBanlist.Contains(user.userId))
            {
                throw new Exception("Error: User has banned.");
            }

            if (user.userPwd != EncryptHelper.SHA256(userpwd))
            {
                throw new Exception("error: wrong password.");
            }

            //2.给token
            string output = $"{username}|{DateTime.Now.ToString("yyyy-MM-dd")}";
            string token = EncryptHelper.AES_Encrypt(output);
            return new LoginFormClientResult { Token = token, Version = Global.NSmartProxyServerName, Userid = user.userId };
        }
        #endregion

        #region users

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userpwd"></param>
        /// <param name="isAdmin">1代表是 0代表否</param>
        [API]
        [Secure]
        public void AddUserV2(string userName, string userpwd, string isAdmin)
        {

            if (Dbop.Exist(userName))
            {
                throw new Exception("error: user exist.");
            }
            var user = new User
            {
                userId = NSmartDbOperator.SUPER_VARIABLE_INDEX_ID,  //索引id
                userName = userName,
                userPwd = EncryptHelper.SHA256(userpwd),
                regTime = DateTime.Now.ToString(),
                isAdmin = isAdmin
            };
            //if (isAdmin == true) user.
            //1.增加用户
            Dbop.Insert(userName, user.ToJsonString());
        }



        [API]
        [Secure]
        public void RemoveUser(string userIndex, string userNames)
        {
            try
            {
                var arr = userIndex.Split(',');
                var userNameArr = userNames.Split(',');
                for (var i = arr.Length - 1; i > -1; i--)
                {
                    Dbop.Delete(int.Parse(arr[i]));
                    Dbop.DeleteHash(userNameArr[i]);
                }

                //删除用户绑定
                lock (userLocker)
                {
                    if (ServerContext.ServerConfig.BoundConfig.UserPortBounds.ContainsKey(userIndex))
                        ServerContext.ServerConfig.BoundConfig.UserPortBounds.Remove(userIndex);
                }
                //刷新绑定列表
                ServerContext.UpdatePortMap();
                ServerContext.ServerConfig.SaveChanges(ServerContext.ServerConfigPath);
            }
            catch (Exception ex)
            {
                throw new Exception("删除用户出错：" + ex.Message);
            }
        }

        [API]
        [Secure]
        public List<string> GetUsers()
        {
            List<string> userStrList = Dbop.Select(0, 999);
            for (int i = 0; i < userStrList.Count; i++)
            {
                var user = userStrList[i].ToObject<UserDTO>();
                var userBounds = ServerContext.ServerConfig.BoundConfig.UserPortBounds;
                if (userBounds.ContainsKey(user.userId))
                {
                    if (userBounds[user.userId].Bound != null)
                        user.boundPorts = string.Join(',', userBounds[user.userId].Bound);
                }
                var banlist = ServerContext.ServerConfig.BoundConfig.UsersBanlist;
                user.isBanned = banlist?.Contains(user.userId).ToString().ToLower();
                //

                user.isOnline = ServerContext.Clients.ContainsKey(int.Parse(user.userId)).ToString().ToLower();

                userStrList[i] = user.ToJsonString();
            }

            return userStrList;
            //}
        }

        [ValidateAPI]
        [Secure]
        public bool ValidateUserName(string username)
        {
            return !Dbop.Exist(username);
        }

        #endregion

        #region connections
        //NoApi Auth
        [API]
        [Secure]
        public string GetClientsInfoJson()
        {
            var connectionManager = ClientConnectionManager.GetInstance();
            StringBuilder json = new StringBuilder("[ ");
            foreach (var (key, value) in ServerContext.PortAppMap)
            {
                json.Append("{ ");
                json.Append(KV2Json("port", key)).C();
                json.Append(KV2Json("clientId", value.ClientId)).C();
                json.Append(KV2Json("appId", value.AppId)).C();
                json.Append(KV2Json("blocksCount", value.TcpClientBlocks.Count)).C();
                //反向连接
                json.Append(KV2Json("revconns"));
                json.Append("[ ");
                foreach (var reverseClient in value.ReverseClients)
                {
                    json.Append("{ ");
                    if (reverseClient.Connected)
                    {
                        json.Append(KV2Json("lEndPoint", reverseClient.Client.LocalEndPoint.ToString())).C();
                        json.Append(KV2Json("rEndPoint", reverseClient.Client.RemoteEndPoint.ToString()));
                    }

                    //json.Append(KV2Json("p", c)).C();
                    //json.Append(KV2Json("port", ca.Key));
                    json.Append("}");
                    json.C();
                }

                json.D();
                json.Append("]").C();

                //隧道状态
                json.Append(KV2Json("tunnels"));
                json.Append("[ ");
                foreach (var tunnel in value.Tunnels)
                {
                    json.Append("{ ");
                    if (tunnel.ClientServerClient != null)
                    {
                        Socket sktClient = tunnel.ClientServerClient.Client;
                        if (tunnel.ClientServerClient.Connected)

                            json.Append(KV2Json("clientServerClient", $"{sktClient.LocalEndPoint}-{sktClient.RemoteEndPoint}"))
                                .C();
                    }
                    if (tunnel.ConsumerClient != null)
                    {
                        Socket sktConsumer = tunnel.ConsumerClient.Client;
                        if (tunnel.ConsumerClient.Connected)
                            json.Append(KV2Json("consumerClient", $"{sktConsumer.LocalEndPoint}-{sktConsumer.RemoteEndPoint}"))
                                .C();
                    }

                    json.D();
                    //json.Append(KV2Json("p", c)).C();
                    //json.Append(KV2Json("port", ca.Key));
                    json.Append("}");
                    json.C();
                }

                json.D();
                json.Append("]");
                json.Append("}").C();
            }

            json.D();
            json.Append("]");
            return json.ToString();
        }

        /// <summary>
        /// 强制断开客户端
        /// </summary>
        /// <param name="clientIdStr"></param>
        /// <returns></returns>
        [API]
        [Secure]
        public bool CloseClient(string clientIdStr)
        {
            if (ServerContext == null) return false;
            var idStrs = clientIdStr.Split(",");

            foreach (var idStr in idStrs)
            {
                var id = int.Parse(idStr);
                ServerContext.CloseAllSourceByClient(id);
            }

            return true;
        }

        private object userLocker = new object();

        /// <summary>
        /// 禁止客户端访问
        /// </summary>
        /// <param name="clientIdStr">用户id字符串，逗号分隔</param>
        /// <param name="addToBanlist">是否加入黑名单 1为加入 0位不加入</param>
        /// <returns></returns>
        [API]
        [Secure]
        public bool BanUsers(string clientIdStr, string addToBanlist = "1")
        {
            if (ServerContext == null) return false;
            var idStrs = clientIdStr.Split(",");

            foreach (var idStr in idStrs)
            {
                var id = int.Parse(idStr);
                ServerContext.CloseAllSourceByClient(id);
            }
            if (addToBanlist.Trim() == "1")
            {
                lock (userLocker)
                {

                    //TODO QQQ 加入禁用列表 需要考虑线程安全
                    foreach (var idStr in idStrs)
                    {
                        ServerContext.ServerConfig.BoundConfig.UsersBanlist.Add(idStr);
                    }
                }
            }

            ServerContext.ServerConfig.SaveChanges(ServerContext.ServerConfigPath);
            //写入数据
            return true;
        }

        [API]
        [Secure]
        public bool UnBanUsers(string clientIdStr)
        {
            if (ServerContext == null) return false;
            var idStrs = clientIdStr.Split(",");

            lock (userLocker)
            {
                //TODO QQQ 剔除禁用列表 需要考虑线程安全
                foreach (var idStr in idStrs)
                {
                    ServerContext.ServerConfig.BoundConfig.UsersBanlist.Remove(idStr);
                }
            }
            ServerContext.ServerConfig.SaveChanges(ServerContext.ServerConfigPath);
            //写入数据
            return true;
        }

        /// <summary>
        /// 将用户和端口绑定，以防被其他用户占用
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ports"></param>
        /// <returns></returns>
        [API]
        [Secure]
        public string BindUserToPort(string userId, string ports)
        {
            List<int> portsList = null;
            try
            {
                if (string.IsNullOrEmpty(ports))
                {
                    portsList = new List<int>();
                }
                else
                {
                    portsList = ports.Split(",").Select(str => int.Parse(str)).ToList();
                }
            }
            catch
            {
                return "字符串格式不正确，只允许逗号分隔的数字";
            }

            //取绑定列表和用户列表的交集
            var userBound = GetUserBounds(userId);
            lock (userLocker)
            {
                var unAvailabelPorts = NetworkUtil.FindUnAvailableTCPPorts(portsList.Except(userBound).ToList());
                if (unAvailabelPorts.Count > 0)
                {
                    string msg = $"端口{string.Join(',', unAvailabelPorts)}无法使用";
                    Server.Logger.Debug(msg);
                    //throw new Exception(msg);
                    return msg;
                }
                //TODO 绑定端口到用户
                var userBounds = ServerContext.ServerConfig.BoundConfig.UserPortBounds;
                userBounds[userId] = new UserPortBound() { Bound = portsList, UserId = userId };
                //TODO QQQ还需要刷新一下端口绑定
                ServerContext.UpdatePortMap();
                ServerContext.ServerConfig.SaveChanges(ServerContext.ServerConfigPath);
            }

            return "操作成功。";
        }

        private List<int> GetUserBounds(string userId)
        {
            var bounds = ServerContext.ServerConfig.BoundConfig.UserPortBounds;
            if (bounds.ContainsKey(userId))
            {
                if (bounds[userId].Bound != null)

                    return bounds[userId].Bound;
            }
            return new List<int>();
        }

        #region  json转换用的私有方法

        private string KV2Json(string key)
        {
            return "\"" + key + "\":";
        }
        private string KV2Json(string key, object value)
        {
            return "\"" + key + "\":\"" + value.ToString() + "\"";
        }

        #endregion

        #endregion






    }
}
