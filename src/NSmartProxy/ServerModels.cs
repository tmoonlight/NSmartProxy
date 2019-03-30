using System;
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

    public class NSPClientCollection
    {
        private Dictionary<int, NSPClient> ClientMap;

        public NSPClient this[int index]
        {
            get => ClientMap[index];
            set => ClientMap[index] = value;
        }
        public NSPClientCollection()
        {
            ClientMap = new Dictionary<int, NSPClient>();
        }

        public bool ContainsKey(int key)
        {
            return ClientMap.ContainsKey(key);
        }

        public void RegisterNewClient(int key)
        {
            if (!ClientMap.ContainsKey(key))
                ClientMap[key] = new NSPClient()
                {
                    ClientID = key,
                    LastUpdateTime = DateTime.Now.Ticks
                };
        }

    }


    public class AppChangedEventArgs : EventArgs
    {
        public NSPApp App;
    }

}