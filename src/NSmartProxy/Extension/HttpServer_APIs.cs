using NSmartProxy.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NSmartProxy.Extension
{
    /// <summary>
    /// 这里存放API
    /// </summary>
    partial class HttpServer
    {
        //TODO XXXX
        #region  dashboard


        #endregion

        #region log
        [API]
        public string[] GetLogInfo(int fromline, int lines)
        {
            //取第X到Y行的日志
            return null;
        }

        [FileAPI]
        public string[] GetLogInfo(string filekey)
        {
            string suffix = ".log";
            //通过日志文件名获取文件
            return null;
        }
        #endregion

        #region config

        private const string C_OPEN_DASH_BOARD = nameof(C_OPEN_DASH_BOARD);
        private const string C_OPEN_LOG_TRACK = nameof(C_OPEN_LOG_TRACK);
        private const string C_CLIENT_SERVICE_PORT = "ClientServicePort";
        private const string C_CONFIG_SERVICE_PORT = "ConfigServicePort";
        private const string C_WEB_API_PORT = "WebAPIPort";
        //API SetConfig
        //API GetConifgs

        #endregion

        #region login
        [FormAPI]
        public string Login(string userid, string userpwd)
        {

            //1.校验
            dynamic user = Dbop.Get(long.Parse(userid))?.ToDynamic();
            if (user == null)
            {
                return "error: user not exist.";
            }
            if (user.userPwd != EncryptHelper.SHA256(userpwd))
            {
                return "error: wrong password.";
            }

            //2.给token
            string output = $"{userid}|{DateTime.Now.ToString("yyyy-MM-dd")}";
            string token = EncryptHelper.AES_Encrypt(output);
            return string.Format(@"
<html>
<head><script>
document.cookie='NSPTK={0}';
document.write('跳转中...');
window.location.href='main.html';
</script>
</head>
</html>
            ", token);
        }
        #endregion

        #region users
        [API]
        public void AddUser(string userid, string userpwd)
        {

            if (Dbop.Exist(userid))
            {
                throw new Exception("error: user exist.");
            }
            var user = new { userId = userid, userPwd = EncryptHelper.SHA256(userpwd), regTime = DateTime.Now.ToString() };
            //1.增加用户
            Dbop.Insert(long.Parse(userid), user.ToJsonString());
        }



        [API]
        public void RemoveUser(string userIndex)
        {
            try
            {
                var arr = userIndex.Split(',');
                for (var i = arr.Length - 1; i > -1; i--)
                {
                    Dbop.Delete(int.Parse(arr[i]));
                }

            }
            catch (Exception ex)
            {
                throw new Exception("删除用户出错：" + ex.Message);
            }
        }

        [API]
        public List<string> GetUsers()
        {
            //using (var dbop = Dbop.Open())
            //{
            return Dbop.Select(0, 10);
            //}
        }
        #endregion

        #region connections
        //NoApi Auth
        [API]
        public string GetClientsInfoJson()
        {
            var ConnectionManager = ClientConnectionManager.GetInstance();
            StringBuilder json = new StringBuilder("[ ");
            foreach (var (key, value) in ConnectionManager.PortAppMap)
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
                ;

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
        #region  私有方法

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
