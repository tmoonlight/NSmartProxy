using NSmartProxy.Data;
using NSmartProxy.Infrastructure;
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
    public class NetworkUtil
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


        public static async Task<TcpClient> ConnectAndSend(string addess, int port, Protocol protocol, byte[] data, bool isClose = false)
        {
            TcpClient configClient = new TcpClient();
            var delayDispose = Task.Delay(TimeSpan.FromSeconds(30)).ContinueWith(_ => configClient.Dispose());
            var connectAsync = configClient.ConnectAsync(addess, port);
            //超时则dispose掉
            var comletedTask = await Task.WhenAny(delayDispose, connectAsync);
            if (!connectAsync.IsCompleted)
            {
                throw new Exception("连接超时");
            }

            var configStream = configClient.GetStream();
            await configStream.WriteAsync(new byte[] { (byte)protocol }, 0, 1);
            await configStream.WriteAndFlushAsync(data, 0, data.Length);
            Console.Write(protocol.ToString() + " proceed.");
            if (isClose)
                configClient.Close();
            return configClient;
        }

    }
}
