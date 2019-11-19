//作者：Mcdull
//说明：FTP命令请求接收处理类
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace FtpServer
{
    class FtpRequest : IDisposable
    {
        private RequestType transferType;
        private string fileName;
        private FtpClient client;

        private TcpListener PASV_listener;  //PASV模式启用监听
        private readonly IPAddress PASV_PROXY_IP;
        private readonly int PASV_PROXY_PORT;
        private readonly IPAddress PASV_IP;
        private readonly int PASV_PORT = 5397;
        private IPAddress PORT_IP;          //PORT模式记录客户端监听地址和端口
        private int PORT_PORT;

        public FtpRequest(FtpClient client, IPAddress pasv_ip, int pasv_port)
        {
            transferType = RequestType.PORT;
            this.client = client;
            this.PASV_IP = this.PASV_PROXY_IP = pasv_ip;
            this.PASV_PORT = this.PASV_PROXY_PORT = pasv_port;
        }

        public FtpRequest(FtpClient client, IPAddress pasv_ip, int pasv_port, IPAddress pasv_proxy_ip, int pasv_proxy_port) : this(client, pasv_ip, pasv_port)
        {
            this.PASV_PROXY_IP = pasv_proxy_ip;
            this.PASV_PROXY_PORT = pasv_proxy_port;
        }

        public RequestType Handle(string[] tokens)
        {
            if (tokens == null || tokens.Length < 1)
            {
                return RequestType.ERROR;
            }
            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = tokens[i].Trim("\0".ToCharArray());
                tokens[i] = tokens[i].Trim("\r\n".ToCharArray());
            }
            if (tokens[0].ToUpper().IndexOf("XPWD") > -1)
                tokens[0] = "XPWD";
            Console.WriteLine("处理命令：" + tokens[0]);
            switch (tokens[0].ToUpper())
            {
                case "USER":
                    client.user.username = tokens[1];
                    return RequestType.LOGIN_USER;
                case "PASS":
                    client.user.password = tokens[1];
                    return RequestType.LOGIN_PASS;
                case "SYST":
                    client.SendMessage("215 " + Environment.OSVersion.ToString());
                    return RequestType.SYSTEM;
                case "OPTS":
                    client.SendMessage("200 设置成功");
                    var args = tokens[1].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    client.UpdateEncode(args[0], args[1]);
                    return RequestType.OPTS;
                case "RETR":
                    fileName = tokens[1];
                    byte[] file = client.GetFile(fileName);
                    if (file == null)
                    {
                        client.SendMessage("550 文件不存在");
                    }
                    else if (file.Length == 0)
                    {
                        client.SendMessage("550 文件大小为空");
                    }
                    else
                    {
                        client.SendMessage("150 开始传输数据");
                        client.SendMessageByTempSocket(getTempSocket(), file);
                        client.SendMessage("226 文件发送完毕");
                    }
                    return RequestType.RETRIEVE;
                case "STOR":
                    fileName = tokens[1];
                    client.SendMessage("150 开始传输数据");
                    if (client.ReceiveFile(getTempSocket(), fileName))
                        client.SendMessage("226 文件接受完毕");
                    else
                        client.SendMessage("550 不能上传文件(文件可能已存在)");
                    return RequestType.STORE;
                case "RNFR":
                    fileName = tokens[1];
                    client.SendMessage("350");
                    return RequestType.RENAME_FROM;
                case "RNTO":
                    int r = client.Rename(fileName, tokens[1]);
                    fileName = tokens[1];
                    switch (r)
                    {
                        case 1:
                            client.SendMessage("250 目录改名成功");
                            break;
                        case 2:
                            client.SendMessage("250 文件改名成功");
                            break;
                        default:
                            client.SendMessage("550 目录或者文件不存在");
                            break;
                    }
                    return RequestType.RENAME_TO;
                case "XMKD":
                case "MKD":
                    fileName = tokens[1];
                    if (!client.CreateDir(fileName))
                        client.SendMessage("221 目录已经存在");
                    else
                        client.SendMessage("250 目录创建成功");
                    return RequestType.XMKD;
                case "DELE":
                    fileName = tokens[1];
                    switch (client.Delete(fileName))
                    {
                        case 1:
                            client.SendMessage("250 目录删除成功");
                            break;
                        case 2:
                            client.SendMessage("250 文件删除成功");
                            break;
                        default:
                            client.SendMessage("221 目录或者文件不存在");
                            break;
                    }
                    return RequestType.DELETE;
                case "PWD":
                case "XPWD":
                    client.SendMessage("257 当前目录\"" + client.GetCurrentDir() + "\"");
                    return RequestType.PWD;
                case "LIST":
                case "NLST":
                    client.SendMessage("150 显示目录信息");
                    client.SendMessageByTempSocket(getTempSocket(), client.GetCurrentDirList());
                    client.SendMessage("226 显示完毕");
                    return RequestType.LIST;
                case "CWD":
                    fileName = tokens[1];
                    if (client.GotoDir(fileName))
                        client.SendMessage("257 当前目录\"" + client.GetCurrentDir() + "\"");
                    else
                        client.SendMessage("500 目录不存在");
                    return RequestType.CWD;
                case "CDUP":
                    if (client.GotoDir(".."))
                        client.SendMessage("257 当前目录\"" + client.GetCurrentDir() + "\"");
                    else
                        client.SendMessage("500 目录不存在");
                    return RequestType.CDUP;
                case "NOOP":
                    client.SendMessage("200 NOOP命令成功");
                    return RequestType.NOOP;
                case "QUIT":
                    client.SendMessage("221 退出登录");
                    client.LoginOut();
                    return RequestType.LOGOUT;
                case "PORT":
                    {
                        transferType = RequestType.PORT;
                        string[] data = new string[6];
                        if (tokens.Length == 2)
                            data = tokens[1].Split(new char[] { ',' });
                        //else if (tokens.Length == 3)
                        //    data = tokens[2].Split(new char[] { ',' });
                        else
                            throw new ArgumentException("PORT命令参数无效");
                        PORT_PORT = (Int32.Parse(data[4]) << 8) + Int32.Parse(data[5]);
                        PORT_IP = IPAddress.Parse(data[0] + "." + data[1] + "." + data[2] + "." + data[3]);
                        client.SendMessage("200");
                        return RequestType.DATA_PORT;
                    }
                case "PASV":
                    {
                        transferType = RequestType.PASV;
                        if (PASV_listener == null)
                        {
                            PASV_listener = new TcpListener(PASV_IP, PASV_PORT);
                            PASV_listener.Start();
                        }
                        string ip = string.Format("{0},{1},{2}", PASV_PROXY_IP.ToString().Replace('.', ','), PASV_PROXY_PORT >> 8, PASV_PROXY_PORT & 0xff);
                        client.SendMessage(string.Format("227 Entering Passive Mode ({0})", ip));
                        return RequestType.PASSIVE;
                    }
                default:
                    client.SendMessage("221 未知的命令" + tokens[0].ToUpper());
                    return RequestType.UNKNOWN_CMD;
            }
        }

        private Socket getTempSocket()
        {
            Socket tempSocket = null;
            if (transferType == RequestType.PASV)
            {
                int timeout = 5000;
                while (timeout-- > 0)
                {
                    if (PASV_listener.Pending())
                    {
                        tempSocket = PASV_listener.AcceptSocket();
                        break;
                    }
                    System.Threading.Thread.Sleep(500);
                }
            }
            else
            {
                tempSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tempSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 5000);
                IPEndPoint hostEndPoint = new IPEndPoint(PORT_IP, PORT_PORT);
                try
                {
                    tempSocket.Connect(hostEndPoint);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("PORT连接失败：{0}", ex.Message);
                    return null;
                }
            }
            return tempSocket;
        }

        public void Dispose()
        {
            if (PASV_listener != null)
            {
                PASV_listener.Stop();
            }
        }
    }

    enum RequestType
    {
        /// <summary>
        /// 发送登录名
        /// </summary>
        LOGIN_USER,
        /// <summary>
        /// 发送登录密码
        /// </summary>
        LOGIN_PASS,
        /// <summary>
        /// 请求系统类型
        /// </summary>
        SYSTEM,
        RESTART,
        /// <summary>
        /// 请求获得文件
        /// </summary>
        RETRIEVE,
        /// <summary>
        /// 存储文件
        /// </summary>
        STORE,
        /// <summary>
        /// 要重命名的文件
        /// </summary>
        RENAME_FROM,
        /// <summary>
        /// 重命名为新文件
        /// </summary>
        RENAME_TO,
        ABORT,
        /// <summary>
        /// 删除文件
        /// </summary>
        DELETE,
        /// <summary>
        /// 创建目录
        /// </summary>
        XMKD,
        /// <summary>
        /// 显示当前工作目录
        /// </summary>
        PWD,
        /// <summary>
        /// 请求获得目录信息
        /// </summary>
        LIST,
        /// <summary>
        /// 等待(NOOP)  此命令不产生什么实际动作，它仅使服务器返回OK。
        /// </summary>
        NOOP,
        /// <summary>
        /// 表示类型
        /// </summary>
        REPRESENTATION_TYPE,
        /// <summary>
        /// 退出登录
        /// </summary>
        LOGOUT,
        /// <summary>
        /// 客户端告知服务器端口
        /// </summary>
        DATA_PORT,
        /// <summary>
        /// 采用PASV传输方法（理解为客户端不发送PORT命令，即服务器不能确定客户端口）
        /// </summary>
        PASSIVE,
        /// <summary>
        /// 改变工作目录
        /// </summary>
        CWD,
        /// <summary>
        /// 返回上级目录
        /// </summary>
        CDUP,
        /// <summary>
        /// 设置传输编码
        /// </summary>
        OPTS,
        CHANGE_DIR_UP,
        UNKNOWN_CMD,
        PORT,
        PASV,
        ERROR
    }
}
