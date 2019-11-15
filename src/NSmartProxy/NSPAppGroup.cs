using System;
using System.Collections;
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
    public class NSPAppGroup : Dictionary<string, NSPApp>
    {
        public new void Add(string key, NSPApp value)
        {
            key = key.Replace(" ", "");
            _activateApp = value;
            ProtocolInGroup = value.AppProtocol;
            base.Add(key, value);
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

            foreach (var key in base.Keys)
            {
                if (!this[key].IsClosed)
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
