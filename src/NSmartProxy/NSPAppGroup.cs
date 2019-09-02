using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NSmartProxy.Data;

namespace NSmartProxy
{
    /// <summary>
    /// 按端口区分的app组，
    /// </summary>
    public class NSPAppGroup : IDictionary<string, NSPApp>
    {
        private readonly IDictionary<string, NSPApp> _dict;
        public NSPApp ActivateApp;
        public Protocol ProtocolInGroup;//组协议

        public NSPAppGroup()
        {
            _dict = new Dictionary<string, NSPApp>();
        }

        public IEnumerator<KeyValuePair<string, NSPApp>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        public void Add(KeyValuePair<string, NSPApp> item)
        {
            ActivateApp = item.Value;
            _dict.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<string, NSPApp> item)
        {
            return _dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, NSPApp>[] array, int arrayIndex)
        {
            _dict.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, NSPApp> item)
        {
            return _dict.Remove(item.Key);
        }

        public int Count { get; }
        public bool IsReadOnly { get; }

        public void Add(string key, NSPApp value)
        {
            ActivateApp = value;
            _dict.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _dict.Remove(key);
        }

        public bool TryGetValue(string key, out NSPApp value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public NSPApp this[string key]
        {
            get => _dict[key];
            set { ActivateApp = value; _dict[key] = value; }
        }

        public ICollection<string> Keys
        {
            get { return _dict.Keys; }
        }

        public ICollection<NSPApp> Values
        {
            get { return _dict.Values; }
        }
    }
}
