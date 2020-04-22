using NSmartProxy.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NSmartProxy.Authorize;
using NSmartProxy.Data;
using NSmartProxy.Data.DTOs;
using NSmartProxy.Data.DBEntities;
using NSmartProxy.Database;
using NSmartProxy.Infrastructure.Extension;
using NSmartProxy.Infrastructure.Interfaces;
using NSmartProxy.Shared;

namespace NSmartProxy.Extension
{
    /// <summary>
    /// 这里存放API
    /// </summary>
    partial class HttpServerApis : IWebController
    {
        public const string SUPER_VARIABLE_INDEX_ID = "$index_id$";
        private NSPServerContext ServerContext;
        private HttpListenerContext HttpContext;
        private IDbOperator Dbop;
        private string baseLogFilePath;

        public HttpServerApis(IServerContext serverContext, IDbOperator dbOperator, string logfilePath)
        {
            ServerContext = (NSPServerContext)serverContext;
            Dbop = dbOperator;
            baseLogFilePath = logfilePath;

            //如果库中没有任何记录，则增加默认用户
            if (Dbop.GetLength() < 1)
            {
                AddUserV2("admin", "admin", "1");
            }
        }

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

        [Secure]
        [API]
        public UserStatusDTO GetUserStatus()
        {
            //Dbop.Close();
            int totalCount = Dbop.GetCount();
            var banCount = ServerContext.ServerConfig.BoundConfig.UsersBanlist.Count;
            var onlineCount = ServerContext.Clients.Count();
            var restCount = totalCount - banCount - onlineCount;

            UserStatusDTO dto = new UserStatusDTO
            {
                onlineUsersCount = onlineCount,
                offlineUsersCount = restCount,
                banUsersCount = banCount
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
            string fileFullPath = baseLogFilePath + "/" + filekey;
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

        private const string AllowAnonymousUser = nameof(AllowAnonymousUser);

        [API]
        [Secure]
        public string SetConfig(string key, string value)
        {
            switch (key)
            {
                case AllowAnonymousUser:
                    ServerContext.SupportAnonymousLogin = (value == "1");
                    ServerContext.SaveConfigChanges();
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

        [API]
        public ServerPortsDTO GetServerPorts()
        {
            var config = ServerContext.ServerConfig;
            return new ServerPortsDTO()
            {
                ReversePort = config.ReversePort_Out > 0 ? config.ReversePort_Out : config.ReversePort,
                ConfigPort = config.ConfigPort_Out > 0 ? config.ConfigPort_Out : config.ConfigPort,
                WebAPIPort = config.WebAPIPort
            };
        }

        #endregion

        #region login

        [API]
        public string GetVersionInfo()
        {
            //return $"Server:{Global.NSmartProxyServerName} Client:{Global.NSmartProxyClientName}";
            return NSPVersion.NSmartProxyServerName;
        }

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
document.cookie='{0}={1}; path=/;';
document.write('Redirecting...');
window.location.href='main.html';
</script>
</head>
</html>
            ", Global.TOKEN_COOKIE_NAME, token);
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
                    userId = SUPER_VARIABLE_INDEX_ID,  //索引id
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
            return new LoginFormClientResult { Token = token, Version = NSPVersion.NSmartProxyServerName, Userid = user.userId };
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
                userId = SUPER_VARIABLE_INDEX_ID,  //索引id
                userName = userName,
                userPwd = EncryptHelper.SHA256(userpwd),
                regTime = DateTime.Now.ToString(),
                isAdmin = isAdmin
            };
            //if (isAdmin == true) user.
            //1.增加用户
            Dbop.Insert(userName, user.ToJsonString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userpwd"></param>
        /// <param name="isAdmin">1代表是 0代表否</param>
        [API]
        [Secure]
        public void UpdateUser(string oldUserName, string newUserName, string userPwd, string isAdmin)
        {

            if (!Dbop.Exist(oldUserName))
            {
                throw new Exception($"error: user {oldUserName} not exist.");
            }
            if (newUserName != oldUserName && Dbop.Exist(newUserName))
            {
                throw new Exception($"error: user {newUserName} exist.");
            }
            //var user = new User
            //{
            //    userId = SUPER_VARIABLE_INDEX_ID,  //索引id
            //    userName = userName,
            //    userPwd = EncryptHelper.SHA256(userpwd),
            //    regTime = DateTime.Now.ToString(),
            //    isAdmin = isAdmin
            //};
            User user = Dbop.Get(oldUserName)?.ToObject<User>();
            user.isAdmin = isAdmin;
            user.userName = newUserName;
            if (userPwd != "XXXXXXXX")
            {
                user.userPwd = EncryptHelper.SHA256(userPwd);
            }

            //if (isAdmin == true) user.
            //1.增加用户
            Dbop.UpdateByName(oldUserName, newUserName, user.ToJsonString());
        }



        [API]
        [Secure]
        public void RemoveUser(string userIndex, string userNames)
        {
            try
            {
                var arr = userIndex.Split(',');
                var userNameArr = userNames.Split(',');
                //for (var i = arr.Length - 1; i > -1; i--)
                //{
                //    Dbop.Delete(int.Parse(arr[i]));
                //    Dbop.DeleteHash(userNameArr[i]);
                //}

                //删除用户绑定
                lock (userLocker)
                {
                    if (ServerContext.ServerConfig.BoundConfig.UserPortBounds.ContainsKey(userIndex))
                        ServerContext.ServerConfig.BoundConfig.UserPortBounds.Remove(userIndex);
                }
                //刷新绑定列表
                ServerContext.UpdatePortMap();
                ServerContext.ServerConfig.SaveChanges(ServerContext.ServerConfigPath);
                for (var i = arr.Length - 1; i > -1; i--)
                {
                    var userId = int.Parse(arr[i]);
                    var userDto = Dbop.Get(userNameArr[i]).ToObject<UserDTO>();
                    Dbop.Delete(userId);//litedb不起作用
                    Dbop.DeleteHash(userNameArr[i]);
                    ServerContext.CloseAllSourceByClient(int.Parse(userDto.userId));
                }
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
        public bool ValidateUserName(string isEdit, string oldUsername, string newUserName)
        {
            if (isEdit == "1" && oldUsername == newUserName)
            {
                return true;
            }

            return !Dbop.Exist(newUserName);

        }

        [API]
        [Secure]
        public NSPClientConfig GetServerClientConfig(string userId = null)
        {
            if (String.IsNullOrWhiteSpace(userId))
            {
                var claims =
                    StringUtil.ConvertStringToTokenClaims(HttpContext.Request.Cookies[Global.TOKEN_COOKIE_NAME].Value);
                userId = claims.UserKey;
            }

            var config = Dbop.GetConfig(userId)?.ToObject<NSPClientConfig>();
            return config;
        }

        [API]
        [Secure]
        public void SetServerClientConfig(string userName, string config)
        {
            NSPClientConfig nspClientConfig = null;
            if (String.IsNullOrWhiteSpace(config))//用户如果清空了配置则客户端会自行使用自己的配置文件
            {
                Dbop.SetConfig(userName, "");
            }
            else
            {
                try
                {
                    nspClientConfig = config.ToObject<NSPClientConfig>();
                    nspClientConfig.UseServerControl = true;
                    //nspClientConfig.ProviderAddress = HttpContext.Request.Url.Host;
                    // nspClientConfig.ProviderWebPort = ServerContext.ServerConfig.WebAPIPort;
                    // nspClientConfig.ConfigPort = ServerContext.ServerConfig.ConfigPort;
                    // nspClientConfig.ReversePort = ServerContext.ServerConfig.ReversePort;
                }
                catch (Exception e)
                {
                    throw new Exception("配置格式不正确。");
                }

                Dbop.SetConfig(userName, nspClientConfig.ToJsonString());
            }

            //重置客户端(给客户端发送重定向请求让客户端主动重启)
            var userid = Dbop.Get(userName)?.ToObject<User>().userId;
            //var popClientAsync = await ServerContext.Clients[userid].AppMap.First().Value.PopClientAsync();
            
            ServerContext.CloseAllSourceByClient(int.Parse(userid));

            //ServerContext.CloseAllSourceByClient();
            //return new NSPClientConfig();
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

                if (value.Count > 0)
                {
                    foreach (var (key2, value2) in value)
                    {
                        AddAppJsonItem(json, key, value2);
                    }
                }
                else
                {
                    AddAppJsonItem(json, key, value.ActivateApp);
                }
            }

            foreach (var (key, value) in ServerContext.UDPPortAppMap)
            {

                if (value.Count > 0)
                {
                    foreach (var (key2, value2) in value)
                    {
                        AddAppJsonItem(json, key, value2);
                    }
                }
                else
                {
                    AddAppJsonItem(json, key, value.ActivateApp);
                }
            }

            json.D();
            json.Append("]");
            return json.ToString();
        }

        private void AddAppJsonItem(StringBuilder json, int key, NSPApp value)
        {
            json.Append("{ ");
            json.Append(KV2Json("port", key)).C();
            json.Append(KV2Json("host", value.Host)).C();
            json.Append(KV2Json("clientId", value.ClientId)).C();
            json.Append(KV2Json("appId", value.AppId)).C();
            json.Append(KV2Json("blocksCount", value.TcpClientBlocks.Count)).C();
            json.Append(KV2Json("description", value.Description)).C();
            json.Append(KV2Json("protocol", Enum.GetName(typeof(Protocol), value.AppProtocol))).C();
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
                json.Append("}");
                json.C();
            }

            json.D();
            json.Append("]");
            json.Append("}").C();
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
            return "\"" + key + "\":\"" + value + "\"";
        }

        #endregion

        #endregion

        #region ca
        [API]
        [Secure]
        public List<CertDTO> GetAllCA()
        {
            List<CertDTO> caList = new List<CertDTO>();

            foreach (var (port, cert) in ServerContext.PortCertMap)
            {
                var cert2 = new X509Certificate2(cert);
                caList.Add(new CertDTO()
                {
                    CreateTime = cert2.GetEffectiveDateString(),
                    Port = int.Parse(port),
                    ToTime = cert2.GetExpirationDateString(),
                    Extensions = "证书名称：" + cert2.FriendlyName//FormatExtension(cert2.Extensions)性能太慢了
                });
            }
            return caList;
        }

        private string FormatExtension(X509ExtensionCollection cert2Extensions)
        {
            StringBuilder extStr = new StringBuilder();
            foreach (var ext in cert2Extensions)
            {
                extStr.Append(ext.Oid.FriendlyName + "：");
                extStr.Append(ext.Format(true).Replace("\n\n", "<br />")
                    .Replace("\n", "<br />") + "<br />");

            }

            return extStr.ToString();
        }


        [API]
        [Secure]
        public string GenerateCA(string hosts)
        {
            var caName = RandomHelper.NextString(10, false);
            X509Certificate2 ca = CAGen.GenerateCA(caName, hosts);
            var export = ca.Export(X509ContentType.Pfx);
            string baseLogPath = "./temp";
            string fileName = "_" + caName + ".pfx";
            string targetPath = baseLogPath + "/" + fileName;
            DirectoryInfo dir = new DirectoryInfo(baseLogPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            // File.Move(fileInfo.FullName, baseLogPath + "/" + port + ".pfx");
            File.WriteAllBytes(targetPath, export);
            return fileName;
        }

        [FileUpload]
        [Secure]
        public string UploadTempFile(FileInfo fileInfo)
        {
            string baseLogPath = "./temp";
            string targetPath = baseLogPath + "/" + fileInfo.Name;
            DirectoryInfo dir = new DirectoryInfo(baseLogPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            File.Move(fileInfo.FullName, targetPath);

            return Path.GetFileName(targetPath);
        }

        [API]
        [Secure]
        public string AddCABound(string port, string filename)
        {
            if (!port.IsNum()) throw new Exception("port不是数字");
            int portInt = int.Parse(port);
            string baseCAPath = "./ca";
            DirectoryInfo dir = new DirectoryInfo(baseCAPath);
            if (!dir.Exists)
            {
                dir.Create();
            }
            filename = Path.GetFileName(filename);//安全起见取一下文件名
            string destPath = baseCAPath + "/" + filename;
            File.Move("./temp/" + filename, destPath);
            ServerContext.PortCertMap[portInt.ToString()] = X509Certificate2.CreateFromCertFile(destPath);
            // ServerContext.PortCertMap[port] = new X509Certificate2,
            //     "WeNeedASaf3rPassword", X509KeyStorageFlags.MachineKeySet);
            ServerContext.ServerConfig.CABoundConfig[portInt.ToString()] = destPath;
            ServerContext.SaveConfigChanges();
            return "";
        }

        [API]
        [Secure]
        public string DelCAFile(string filename)
        {
            //if (!port.IsNum()) throw new Exception("port不是数字");
            filename = Path.GetFileName(filename);
            var filePath = "./ca/" + filename;//安全起见取一下文件名
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            //string destPath = "./ca/" + filename;
            //File.Move("./temp/" + filename, destPath);
            //ServerContext.PortCertMap[port] = X509Certificate.CreateFromCertFile(destPath);
            //ServerContext.ServerConfig.CABoundConfig[port] = destPath;
            //删除文件，配置，以及内存中的绑定

            ServerContext.SaveConfigChanges();
            return "";
        }

        [API]
        [Secure]
        public string DelCABound(string port)
        {
            if (!port.IsNum()) throw new Exception("port不是数字");

            //删除文件，配置，以及内存中的绑定
            var filename = Path.GetFileName(ServerContext.ServerConfig.CABoundConfig[port]);
            var filePath = "./ca/" + filename;//安全起见取一下文件名
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            ServerContext.PortCertMap.Remove(port);
            ServerContext.ServerConfig.CABoundConfig.Remove(port);
            ServerContext.SaveConfigChanges();
            return "success";
        }

        #endregion


        /// <summary>
        /// 设置上下文
        /// </summary>
        /// <param name="context"></param>
        public void SetContext(HttpListenerContext context)
        {
            HttpContext = context;
        }
    }
}
