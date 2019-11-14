using NSmartProxy.Data;
using NSmartProxy.Infrastructure;
using NSmartProxy.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSmartProxy
{
    public static class NetworkUtil
    {
        private const string PortReleaseGuid = "23495C95-AEB6-41C6-B8AF-D03025EF0AE6";

        private static HashSet<int> _usedPorts = new HashSet<int>();

        public static bool ReleasePort(int port)
        {
            try
            {
                mutex.WaitOne();
                _usedPorts.Remove(port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally { mutex.ReleaseMutex(); }
        }

        private static readonly Mutex mutex = new Lazy<Mutex>(() =>
            new Mutex(false,
                PortReleaseGuid)).Value;
        /*string.Concat("./Global/", PortReleaseGuid)*/

        /// <summary>
        /// 查找一个端口
        /// </summary>
        /// <param name="startPort"></param>
        /// <returns></returns>
        public static int FindOneAvailableTCPPort(int startPort)
        {
            return NetworkUtil.FindAvailableTCPPorts(startPort, 1)[0];
        }

        /// <summary> 
        /// Check if startPort is available, incrementing and 
        /// checking again if it's in use until a free port is found 
        /// </summary> 
        /// <param name="startPort">The first port to check</param> 
        /// <returns>The first available port</returns> 
        public static int[] FindAvailableTCPPorts(int startPort, int PortCount)
        {
            int[] arrangedPorts = new int[PortCount];
            int port = startPort;
            bool isAvailable = true;

            var mutex = new Mutex(false,
                PortReleaseGuid);
            mutex.WaitOne();　//全局锁
            try
            {
                for (int i = 0; i < PortCount; i++)
                {
                    IPGlobalProperties ipGlobalProperties =
                        IPGlobalProperties.GetIPGlobalProperties();
                    IPEndPoint[] endPoints =
                        ipGlobalProperties.GetActiveTcpListeners();

                    do
                    {
                        if (!isAvailable)
                        {
                            port++;
                            isAvailable = true;
                        }
                        if (_usedPorts.Contains(port))
                        {
                            isAvailable = false;
                            continue;
                        }
                        foreach (IPEndPoint endPoint in endPoints)
                        {//判断规则 ：0000或者三冒号打头，ip占用，才算占用，否则pass
                         //因为linux下 出现192.168.0.106:8787的记录，也会被误判为端口被占用
                            if (endPoint.Address.ToString() == "0.0.0.0" || endPoint.Address.ToString() == ":::")
                            {
                                if (endPoint.Port == port)
                                {
                                    isAvailable = false;
                                    break;
                                }
                            }
                        }

                    } while (!isAvailable && port < IPEndPoint.MaxPort);

                    if (!isAvailable)
                        throw new ApplicationException("Not able to find a free TCP port.");
                    arrangedPorts[i] = port;
                    _usedPorts.Add(port);
                }
                return arrangedPorts;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary> 
        /// 找出正在被使用的端口
        /// </summary> 
        /// <param name="startPort">The first port to check</param> 
        /// <returns>The first available port</returns> 
        public static List<int> FindUnAvailableTCPPorts(List<int> ports)
        {
            //bool isAvailable = true;
            List<int> usedPortList = new List<int>(ports.Count);

            mutex.WaitOne();
            try
            {
                IPGlobalProperties ipGlobalProperties =
                    IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] endPoints =
                    ipGlobalProperties.GetActiveTcpListeners();
                for (int i = ports.Count - 1; i > -1; i--)
                {
                    if (ports[i] > 65535 || ports[i] < 1 || _usedPorts.Contains(ports[i]))
                    {
                        usedPortList.Add(ports[i]);
                        ports.Remove(ports[i]);
                    }
                }
                foreach (IPEndPoint endPoint in endPoints)
                {
                    var thisPort = endPoint.Port;
                    if (ports.Any(p => p == thisPort))
                    {
                        usedPortList.Add(thisPort);
                    }
                }

                return usedPortList.Distinct().ToList();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }


        /// <summary> 
        /// 找UDP空闲端口
        /// </summary> 
        /// <param name="startPort">The first port to check</param> 
        /// <returns>The first available port</returns> 
        public static int FindOneAvailableUDPPort(int startPort)
        {
            int port = startPort;
            bool isAvailable = true;

            var mutex = new Mutex(false, PortReleaseGuid);
            mutex.WaitOne();
            try
            {
                IPGlobalProperties ipGlobalProperties =
                    IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] endPoints =
                    ipGlobalProperties.GetActiveUdpListeners();

                do
                {
                    if (!isAvailable)
                    {
                        port++;
                        isAvailable = true;
                    }

                    foreach (IPEndPoint endPoint in endPoints)
                    {
                        if (endPoint.Port != port)
                            continue;
                        isAvailable = false;
                        break;
                    }

                } while (!isAvailable && port < IPEndPoint.MaxPort);

                if (!isAvailable)
                    throw new ApplicationException("Not able to find a free TCP port.");

                return port;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }


        public static async Task<TcpClient> ConnectAndSend(string addess, int port, ServerProtocol protocol, byte[] data, bool isClose = false)
        {
            TcpClient configClient = new TcpClient();
            bool isConnected = false;
            for (int j = 0; j < 3; j++)
            {
                var delayDispose = Task.Delay(Global.DefaultConnectTimeout).ContinueWith(_ => configClient.Dispose());
                var connectAsync = configClient.ConnectAsync(addess, port);
                //超时则dispose掉
                var completedTask = await Task.WhenAny(delayDispose, connectAsync);
                if (!connectAsync.IsCompleted)
                {
                    Console.WriteLine("ConnectAndSend连接超时,5秒后重试");
                    await Task.Delay(5000);
                }
                else
                {
                    isConnected = true;
                    break;
                }
            }
            if (!isConnected) { Console.WriteLine("重试次数达到限制。"); throw new Exception("重试次数达到限制。"); }

            var configStream = configClient.GetStream();
            await configStream.WriteAsync(new byte[] { (byte)protocol }, 0, 1);
            await configStream.WriteAndFlushAsync(data, 0, data.Length);
            //Console.Write(protocol.ToString() + " proceed.");
            Console.Write("->");
            if (isClose)
                configClient.Close();
            return configClient;
        }

        /// <summary>
        ///linux需要添加或修改以下值以手动实现keepalive
        /// vim /etc/sysctl.conf
        ///net.ipv4.tcp_keepalive_time = 300
        ///net.ipv4.tcp_keepalive_intvl = 10
        ///net.ipv4.tcp_keepalive_probes = 10
        /// sysctl -p+-
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="ErrorMsg"></param>
        public static void SetKeepAlive(this TcpClient tcpClient, out string ErrorMsg)
        {

            ErrorMsg = "";
            // not all platforms support IOControl
            //try
            //{
            //    var keepAliveValues = new KeepAliveValues
            //    {
            //        OnOff = 1,
            //        KeepAliveTime = 300000, // 300 seconds in milliseconds
            //        KeepAliveInterval = 10000 // 10 seconds in milliseconds
            //    };
            //    socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValues.ToBytes(), null);
            //}
            //catch (PlatformNotSupportedException)
            //{
            //    // most platforms should support this call to SetSocketOption, but just in case call it in a try/catch also
            //    try
            //    {
            //        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            //    }
            //    catch (PlatformNotSupportedException)
            //    {
            //        // ignore PlatformNotSupportedException
            //    }
            //}

            //return socket;

            try
            {
                tcpClient.Client.IOControl(IOControlCode.KeepAliveValues, GetKeepAlivePkg(1, 10000, 20000), null);
            }
            catch (PlatformNotSupportedException)
            {
                // most platforms should support this call to SetSocketOption, but just in case call it in a try/catch also
                try
                {
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                }
                catch (PlatformNotSupportedException ex)
                {
                    //平台不支持
                    ErrorMsg = ex.Message;
                }
            }

        }

        /// <summary>
        /// 生成keepalive包用于Socket.IOControl
        /// </summary>
        /// <param name="onOff">是否開啟 Keep-Alive(開 1/ 關 0)</param>
        /// <param name="keepAliveTime">當開啟keep-Alive後經過多久時間(ms)開啟偵測</param>
        /// <param name="keepAliveInterval">多久偵測一次</param>
        /// <returns></returns>
        static byte[] GetKeepAlivePkg(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }

        /// <summary>
        /// 清空排除端口集合
        /// </summary>
        public static void ClearAllUsedPorts()
        {
            try
            {
                mutex.WaitOne();
                _usedPorts = new HashSet<int>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally { mutex.ReleaseMutex(); }
        }

        public static void ReAddUsedPorts(List<int> usedPorts)
        {
            ClearAllUsedPorts();
            AddUsedPorts(usedPorts);
        }

        /// <summary>
        /// 增加排除端口，这些端口永远不会被分配到
        /// </summary>
        /// <param name="usedPorts"></param>
        public static void AddUsedPorts(List<int> usedPorts)
        {
            try
            {
                mutex.WaitOne();
                foreach (var port in usedPorts)
                {
                    if (!_usedPorts.Contains(port))
                        _usedPorts.Add(port);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally { mutex.ReleaseMutex(); }

        }

        /// <summary>
        /// 删除排除端口
        /// </summary>
        /// <param name="usedPorts"></param>
        public static void RemoveUsedPorts(List<int> usedPorts)
        {
            try
            {
                mutex.WaitOne();
                foreach (var port in usedPorts)
                {
                    usedPorts.Remove(port);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally { mutex.ReleaseMutex(); }

        }
    }
}
