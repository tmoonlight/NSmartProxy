
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
        private const string PortReleaseGuid = "C086DE94-2C45-4247-81E2-2E5248F5A769";

        private static HashSet<int> _usedPorts = new HashSet<int>();

        public static bool ReleasePort(int port)
        {
            return _usedPorts.Remove(port);
        }

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
                PortReleaseGuid
                /*string.Concat("./Global/", PortReleaseGuid)*/);
            mutex.WaitOne();
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
                        {
                            if (endPoint.Port != port) continue;
                            isAvailable = false;
                            break;
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
        /// Check if startPort is available, incrementing and 
        /// checking again if it's in use until a free port is found 
        /// </summary> 
        /// <param name="startPort">The first port to check</param> 
        /// <returns>The first available port</returns> 
        public static int FindNextAvailableUDPPort(int startPort)
        {
            int port = startPort;
            bool isAvailable = true;

            var mutex = new Mutex(false,
                string.Concat("Global/", PortReleaseGuid));
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

            //try
            //{
                tcpClient.Client.IOControl(IOControlCode.KeepAliveValues, GetKeepAlivePkg(1, 1000, 20000), null);
            //}
            //catch (PlatformNotSupportedException)
            //{
            //    // most platforms should support this call to SetSocketOption, but just in case call it in a try/catch also
            //    try
            //    {
            //        tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            //    }
            //    catch (PlatformNotSupportedException ex)
            //    {
            //        //平台不支持
            //        ErrorMsg = ex.Message;
            //    }
            //}

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

    }
}
