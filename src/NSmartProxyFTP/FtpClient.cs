//作者：Mcdull
//说明：FTP客户端类，每个客户端封装一个套接字负责接收和返回数据
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Globalization;

namespace FtpServer
{
    class FtpClient
    {
        private Socket currentSocket;
        private Thread thread;
        private FtpRequest request;
        private bool isClosed;
        private Encoding encode;
        private string rootDir;

        public User user;
        public event Action<FtpClient> Quit;
        public event Action<FtpClient> Login;

        public FtpClient(Socket socket, IPAddress pasv_ip, int pasv_port, IPAddress pasv_proxy_ip, int pasv_proxy_port)
        {
            isClosed = false;
            user = new User();
            this.request = new FtpRequest(this, pasv_ip, pasv_port, pasv_proxy_ip, pasv_proxy_port);
            this.currentSocket = socket;
            encode = Encoding.Default;
        }

        public IPAddress IP
        {
            get { return ((IPEndPoint)currentSocket.RemoteEndPoint).Address; }
        }

        public void Start()
        {
            thread = new Thread(() =>
            {
                SendMessage("220 欢迎使用FTP服务器，你已经连上了服务器...");
                var type = request.Handle(receiveMsg()[0].tokens);
                if (type == RequestType.OPTS)
                {
                    if (request.Handle(receiveMsg()[0].tokens) != RequestType.LOGIN_USER)
                    {
                        SendMessage("221 命令错误");
                        close();
                        return;
                    }
                }
                else if (type != RequestType.LOGIN_USER)
                {
                    SendMessage("221 命令错误");
                    close();
                    return;
                }
                SendMessage("331 请输入用户 " + user.username + " 的登录密码");
                if (request.Handle(receiveMsg()[0].tokens) != RequestType.LOGIN_PASS)
                {
                    SendMessage("221 命令错误");
                    close();
                    return;
                }
                var u = FtpServer.Users.SingleOrDefault(p => p.username == user.username && p.password == user.password);
                if (u != null)
                {
                    user.isLogin = true;
                    user.workingDir = rootDir = u.rootDir;
                    SendMessage("230 用户 " + user.username + " 授权登录.");
                    onLogin();
                }
                else
                {
                    SendMessage("530 用户名或者密码错误。");
                    close();
                    return;
                }
                while (user.isLogin && !isClosed)
                {
                    if (currentSocket.Connected) // && currentSocket.Available > 0  为了捕获receiveMsg异常从而关闭连接这里注释掉
                    {
                        var tokens = receiveMsg();
                        foreach (var t in tokens)
                        {
                            request.Handle(t.tokens);
                        }
                    }
                    Thread.Sleep(500);
                }
            });
            thread.Start();
        }

        public void Stop()
        {
            close(false);
            if (thread != null)
                thread.Abort();
        }

        #region Request处理各种请求方法
        //获取发送来的文件
        public bool ReceiveFile(Socket tempSocket, string filename)
        {
            string name = getFileName(filename);
            FileInfo fi = new FileInfo(name);
            if (fi.Exists)
                return false;
            if (tempSocket != null && tempSocket.Connected)
            {
                string dir = Path.GetDirectoryName(name);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                byte[] buffer = new byte[1024];
                using (FileStream fs = new FileStream(name, FileMode.CreateNew, FileAccess.Write, FileShare.Write))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    int length = 0;
                    do
                    {
                        length = tempSocket.Receive(buffer);
                        fs.Write(buffer, 0, length);
                    }
                    while (length > 0);
                    fs.Close();    //这句话可能是多余的
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        //为目录或者文件改名
        public int Rename(string from, string to)
        {
            string name = getFileName(from);
            string new_name = getFileName(to);
            Console.WriteLine("重命名" + name + "到" + new_name);
            FileInfo fi = new FileInfo(name);
            if (Directory.Exists(name))
            {
                Directory.Move(name, new_name);
                return 1;
            }
            else if (fi.Exists)
            {
                fi.MoveTo(new_name);
                return 2;
            }
            return 0;
        }

        //删除文件或目录
        public int Delete(string dirname)
        {
            string dir = getFileName(dirname);
            Console.WriteLine("删除目录/文件" + dir);
            FileInfo fi = new FileInfo(dir);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
                return 1;
            }
            else if (fi.Exists)
            {
                fi.Delete();
                return 2;
            }
            return 0;
        }

        //跳转到其他目录
        public bool GotoDir(string targetDir)
        {
            string dir = trimEnd(targetDir);
            if (dir == "..")
            {
                //转到上一级目录
                if (user.workingDir != rootDir)
                {
                    //如果当前目录不是根目录，就转到上一级目录
                    try
                    {
                        DirectoryInfo di = Directory.GetParent(user.workingDir);
                        if (di != null)
                            user.workingDir = di.FullName;
                        return true;
                    }
                    catch (ArgumentNullException)
                    {
                        Console.WriteLine("路径为空");
                        return false;
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("当前工作路径是一个空字符串，路径中只能包含空格或者其他任何有效的字符。");
                        return false;
                    }
                }
                return true;
            }
            else if (dir == ".")
            {
                return true;
            }
            else if (dir[0] == '/')
            {
                //是否从根目录开始
                dir = dir.TrimStart("/".ToCharArray());
                dir = rootDir + "\\" + dir;
                try
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    if (di.Exists)
                    {
                        user.workingDir = di.FullName;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (NotSupportedException)
                {
                    return false;
                }
            }
            else
            {
                string workingDirName = new DirectoryInfo(user.workingDir).Name;
                var temp = dir.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (temp.Length > 0 && workingDirName == temp[0])
                {
                    int i = dir.IndexOf('/');
                    if (i > 0)
                    {
                        dir = dir.Substring(i + 1, dir.Length - i - 1);
                    }
                }
                //进入目录
                dir = user.workingDir + "\\" + dir;
                try
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    if (di.Exists)
                    {
                        user.workingDir = di.FullName;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (NotSupportedException)
                {
                    return false;
                }
            }
        }

        //创建目录
        public bool CreateDir(string dirName)
        {
            string dir = getFileName(dirName);
            Console.WriteLine("创建目录" + dir);
            if (Directory.Exists(dir))
            {
                return false;
            }
            else
            {
                Directory.CreateDirectory(dir);
                return true;
            }
        }

        //向客户端发送文件
        public byte[] GetFile(string filename)
        {
            string name = getFileName(filename);
            FileInfo fi = new FileInfo(name);
            if (fi.Exists)
            {
                FileStream fs = fi.OpenRead();
                byte[] b = new byte[fs.Length];
                fs.Read(b, 0, b.Length);
                fs.Close();
                return b;
            }
            else
            {
                return null;
            }
        }

        //更新传输编码
        public void UpdateEncode(string name, string mode)
        {
            if (mode.ToUpper() == "ON")
            {
                switch (name.ToUpper())
                {
                    case "UTF8":
                        encode = Encoding.UTF8;
                        break;
                }
            }
            else
                encode = Encoding.Default;
        }

        //列表当前目录文件
        public string GetCurrentDirList()
        {
            string[] dirs = Directory.GetDirectories(user.workingDir);
            string[] files = Directory.GetFiles(user.workingDir);
            string msg = "";
            DateTimeFormatInfo dateTimeFormat = new CultureInfo("en-US", true).DateTimeFormat;
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo fi = new FileInfo(files[i]);
                string name = fi.Name;
                //msg += string.Format("{0:yyyy-MM-dd HH:mm:ss}    {1}    {2}\r\n", fi.LastWriteTimeUtc, fi.Length, name);
                msg += string.Format("-rw-r--r-- 1 root root     {0} {1}  {2} {3:HH:mm} {4}\r\n", fi.Length, dateTimeFormat.GetMonthName(fi.LastWriteTime.Month).Substring(0, 3),
                    fi.LastWriteTime.Day, fi.LastWriteTime, name);
            }

            for (int i = 0; i < dirs.Length; i++)
            {
                DirectoryInfo di = new DirectoryInfo(dirs[i]);
                string name = di.Name;
                //msg += string.Format("{0:yyyy-MM-dd HH:mm:ss}     {1}    {2}\r\n", di.LastWriteTimeUtc, "<DIR>", name);
                msg += string.Format("drw-r--r--    2 0        0            0 {0} {1} {2:HH:mm} {3}\r\n", dateTimeFormat.GetMonthName(di.LastWriteTime.Month).Substring(0, 3),
                  di.LastWriteTime.Day, di.LastWriteTime, name);
            }
            return msg;
        }

        //获取当前目录
        public string GetCurrentDir()
        {
            string dir = user.workingDir;
            dir = dir.Remove(0, rootDir.Length);
            dir = dir.Replace("\\", "/");
            if (dir == "")
                dir = "/";
            else if (dir[0] != '/')
                dir = "/" + dir;
            return dir;
        }

        public void LoginOut()
        {
            user.isLogin = false;
            close();
        }

        /// <summary>
        /// 当前Socket发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(string msg)
        {
            msg += "\r\n";
            sendMsg(encode.GetBytes(msg.ToCharArray()));
        }

        /// <summary>
        /// 根据前一个PORT指定的Socket发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessageByTempSocket(Socket tempSocket, string msg)
        {
            if (tempSocket != null && tempSocket.Connected)
            {
                sendMsg(encode.GetBytes(msg.ToCharArray()), tempSocket);
                //sendMsg(Encoding.Default.GetBytes(msg.ToCharArray()), tempSocket);
                tempSocket.Close();
            }
        }

        public void SendMessageByTempSocket(Socket tempSocket, byte[] msg)
        {
            if (msg.Length > 0)
            {
                if (tempSocket != null && tempSocket.Connected)
                {
                    sendMsg(msg, tempSocket);
                    tempSocket.Close();
                }
            }
        }
        #endregion

        #region 私有方法
        private string getFileName(string name)
        {
            if (name[0] == '/')
            {
                name = name.TrimStart("/".ToCharArray());
                return rootDir + "\\" + name;
            }
            else
                return user.workingDir + "\\" + trimEnd(name);
        }

        private string trimEnd(string str)
        {
            string dir = "";
            int pos = str.IndexOf("\r\n");
            if (pos > -1)
            {
                dir = str.Substring(0, pos);
            }
            else
            {
                dir = str;
            }
            return dir;
        }

        private void sendMsg(Byte[] message, Socket socket = null)
        {
            try
            {
                if (socket == null)
                    currentSocket.Send(message, message.Length, 0);
                else
                    socket.Send(message, message.Length, 0);
            }
            catch
            {

            }
        }

        private List<Token> receiveMsg()
        {
            List<Token> list = new List<Token>();
            byte[] buff = new byte[1024];
            try
            {
                currentSocket.Receive(buff);
                string clientCommand = encode.GetString(buff);
                clientCommand = clientCommand.Trim("\0".ToCharArray());
                clientCommand = clientCommand.Trim("\r\n".ToCharArray());//"PORT 192,168,0,105,51,49\r\nLIST\r\n"这种情况怎么处理，2条命令同时发来
                var msgs = clientCommand.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var msg in msgs)
                {
                    var token = new Token();
                    if (msg.Length > 0)
                    {
                        var index = msg.IndexOf(" ");
                        if (index > -1)
                        {
                            // token.tokens = msg.Split(new char[] { ' ' });
                            token.tokens = new string[2]{
                             msg.Substring(0, index),
                             msg.Substring(index+1, msg.Length- index-1)
                            };

                        }
                        else
                            token.tokens = new string[] { msg };
                    }
                    list.Add(token);
                }
            }
            catch
            {
                close();
            }
            return list;
        }

        private void close(bool @event = true)
        {
            if (!isClosed)
            {
                isClosed = true;
                currentSocket.Close();
                request.Dispose();
                if (@event)
                {
                    var temp = Quit;
                    if (temp != null)
                        temp(this);
                }
            }
        }

        private void onLogin()
        {
            var temp = Login;
            if (temp != null)
                temp(this);
        }

        sealed class Token
        {
            internal string[] tokens { get; set; }
        }
        #endregion
    }
}
