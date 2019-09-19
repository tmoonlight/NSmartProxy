using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using NSmartProxy.Data;

namespace NSmartProxy
{
    /// <summary>
    /// 按端口区分的app组
    /// </summary>
    public class NSPAppGroup : Dictionary<string, NSPApp>
    {
        public new void Add(string key, NSPApp value)
        {
            key = key.Replace(" ", "");
            ActivateApp = value;
            ProtocolInGroup = value.AppProtocol;
            base.Add(key, value);
        }

        public new NSPApp this[string key]
        {
            get => base[key.Replace(" ", "")];
            set
            {
                ActivateApp = value;
                ProtocolInGroup = value.AppProtocol; base[key.Replace(" ", "")] = value;
            }
        }

        public NSPApp ActivateApp;
        public Protocol ProtocolInGroup;//组协议

        public new void Clear()
        {
            ActivateApp = null;
            base.Clear();
        }

        public void CloseAll()
        {
            
            foreach (var key in base.Keys)
            {
                this[key].Close();
            }

            ActivateApp?.Close();
        }
    }
}
