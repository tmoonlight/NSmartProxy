using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Text;

namespace NSmartProxy.Database
{
    /// <summary>
    /// NSmartDb，只是为了造轮子
    /// </summary>
    public class NSmartDbOperator : IDbOperator
    {

        public const string SUPER_VARIABLE_INDEX_ID = "$index_id$";
        private SequenceFile seqf;
        private HashFile hashf;
        private string hashfFile;
        private string seqfFile;

        private bool isClosed = true;//默认未开启状态

        public bool IsClosed { get => isClosed; set => isClosed = value; }

        public NSmartDbOperator(String file, string indexFile)
        {
            // if (indexFile == null) indexFile = "idx_" + file;
            hashfFile = file;
            seqfFile = indexFile;
            Open();
        }

        public IDbOperator Open()
        {
            if (isClosed)
            {
                seqf = new SequenceFile(hashfFile);
                hashf = new HashFile(seqfFile);
                isClosed = false;
            }

            return this;
        }

        public void Insert(long key, string value)
        {
            byte[] keyBytesbytes = BitConverter.GetBytes(key);
            if (hashf.Get(keyBytesbytes) != null) throw new Exception($"cant insert because hashfile has this item:{key}");
            byte[] valBytes = String2Bytes(value);
            //1.插入hash文件
            hashf.Put(keyBytesbytes, valBytes);
            //2.插入序列
            seqf.Add(key);
        }

        /// <summary>
        /// 插入以key为索引的数据，序列id随机
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Insert(string key, string value)
        {
            byte[] keyBytesbytes = String2Bytes(key);
            if (hashf.Get(keyBytesbytes) != null) throw new Exception($"cant insert because hashfile has this item:{key}");
            //byte[] valBytes = String2Bytes(value);
            long index = GetAvailableKey(65535);
            //1.插入索引
            hashf.Put(keyBytesbytes, BitConverter.GetBytes(index));
            //2.插入实际对象, 替换关键标识参数
            Insert(index, value.Replace(SUPER_VARIABLE_INDEX_ID, index.ToString()));
            //seqf.Add(key);
        }

        private Random rand = new Random();

        /// <summary>
        /// 获取可用的随机索引
        /// </summary>
        /// <returns></returns>
        private long GetAvailableKey(int maxValue)
        {
            int key = 0;
            while (true)
            {
                key = rand.Next(65535);
                if (!Exist(key))
                {
                    return key;
                }
            }
        }

        public void Update(long key, string value)
        {
            byte[] keyBytesbytes = BitConverter.GetBytes(key);
            if (hashf.Get(keyBytesbytes) == null) throw new Exception($"cant update because hashfile hasn't this item:{key}");
            byte[] valBytes = String2Bytes(value);
            //1.插入hash文件
            hashf.Put(keyBytesbytes, valBytes);
        }

        public void UpdateByName(string userName, string newUserName, string value)
        {
            throw new NotImplementedException();
        }

        public void UpdateByName(string userName, string value)
        {
            throw new NotImplementedException();
        }

        public List<string> Select(int startIndex, int length)
        {
            List<string> strs = new List<string>(length);
            //获取索引
            var list = seqf.GetRange(startIndex, length);
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] != 0)
                {
                    var hashValue = hashf.Get
                    (BitConverter.GetBytes(list[i])
                     ?? new byte[] { 0 });
                    if (hashValue != null)
                    {
                        strs.Add(Bytes2String(hashValue));
                    }
                }
            }

            return strs;
            //输出
        }

        public string GetConfig(string userId)
        {
            throw new NotImplementedException();
        }

        public void SetConfig(string userId, string config)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 通过序列删除数据，并且返回id
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public void Delete(int index)
        {
            var key = seqf.Get(index);
            seqf.Delete(index);

            hashf.Remove(BitConverter.GetBytes(key));
        }

        public void DeleteHash(string key)
        {
            hashf.Remove(String2Bytes(key));
        }

        public string GetOne(string key)
        {
            return Bytes2String(hashf.Get(String2Bytes(key)));
        }

        public bool Exist(string key)
        {
            return hashf.Exist(String2Bytes(key));
        }

        public int GetCount()
        {
            throw new NotImplementedException();
        }

        public bool Exist(long key)
        {
            return hashf.Exist(BitConverter.GetBytes(key));
        }

        public long GetLength()
        {
            return seqf.Get(-1);
        }

        public void Close()
        {
            if (!isClosed)
            {
                seqf.Close();
                hashf.Close();
                IsClosed = true;
            }
        }

        private string Bytes2String(byte[] bytes)
        {
            return ASCIIEncoding.ASCII.GetString(bytes);
        }

        private byte[] String2Bytes(string str)
        {
            return ASCIIEncoding.ASCII.GetBytes(str);
        }

        public void Dispose()
        {
            this.Close();
        }

        public string Get(long key)
        {
            return Bytes2String(hashf.Get(BitConverter.GetBytes(key)));
        }

        public string Get(string key)
        {
            var point2Value = hashf.Get(String2Bytes(key));
            if (point2Value == null) return null;
            return Bytes2String(hashf.Get(point2Value));
        }
    }
}
