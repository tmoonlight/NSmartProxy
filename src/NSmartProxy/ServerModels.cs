using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NSmartProxy.Infrastructure;

namespace NSmartProxy
{
    public struct ClientIDAppID
    {
        public int ClientID;
        public int AppID;
    }

    /// <summary>
    /// nspclient集合 clientid->NSPClient
    /// </summary>
    public class NSPClientCollection:IEnumerable<NSPClient>
    {
        private ConcurrentDictionary<int, NSPClient> ClientMap;

        public NSPClient this[int index]
        {
            get => ClientMap[index];
            set => ClientMap[index] = value;
        }
        public NSPClientCollection()
        {
            ClientMap = new ConcurrentDictionary<int, NSPClient>();
        }

        public bool ContainsKey(int key)
        {
            return ClientMap.ContainsKey(key);
        }

        public void RegisterNewClient(int key)
        {
            
                ClientMap.TryAdd(key,new NSPClient()
                {
                    ClientID = key,
                    LastUpdateTime = DateTime.Now
                });
        }

        public void UnRegisterClient(int key)
        {
            //关闭所有连接
            //int closedClients = ClientMap[key].Close();
            this.ClientMap.TryRemove(key,out _);
            //停止端口侦听
            //return closedClients;
        }

        public IEnumerator<NSPClient> GetEnumerator()
        {
            return ClientMap.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class AppChangedEventArgs : EventArgs
    {
        public NSPApp App;
    }

}