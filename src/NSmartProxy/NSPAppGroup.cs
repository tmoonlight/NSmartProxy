using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NSmartProxy.Data;

namespace NSmartProxy
{
    /// <summary>
    /// 按host区分的app组，ActivateApp始终是最后一个进来的app。
    /// 一个app组中所有的app的协议必须相同。
    /// </summary>
    //基类改为ConcurrentDictionary：这个字典在客户端上下线时被写入，
    //同时被每一个外部连接读取，普通Dictionary并发读写可能损坏内部结构、
    //导致查找陷入死循环（表现为CPU 100%且所有请求卡死）。
    public class NSPAppGroup : ConcurrentDictionary<string, NSPApp>
    {
        public new void Add(string key, NSPApp value)
        {
            key = key.Replace(" ", "");
            _activateApp = value;
            ProtocolInGroup = value.AppProtocol;
            base[key] = value;
        }

        public new NSPApp this[string key]
        {
            get => base[key.Replace(" ", "")];
            set
            {
                _activateApp = value;
                ProtocolInGroup = value.AppProtocol; base[key.Replace(" ", "")] = value;
            }
        }

        public NSPApp _activateApp;

        public NSPApp ActivateApp
        {
            get { return _activateApp; }
           // set { _activateApp = value; }
        }
        public Protocol ProtocolInGroup;//组协议

        public new void Clear()
        {
            _activateApp = null;
            base.Clear();
        }

        public bool IsAllClosed()
        {
            //直接遍历Values：先取Keys再按key回查，在并发移除时会抛KeyNotFoundException
            foreach (var app in base.Values)
            {
                if (!app.IsClosed)
                {
                    return false;
                }
            }
            return true;
        }

        public TcpListener Listener { get; set; }
        public UdpClient UdpClient { get; set; }
        public Task UdpTransmissionTask { get; set; }
    }
}
