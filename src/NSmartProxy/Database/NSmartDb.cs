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
    public class NSmartDb
    {
        private SequenceFile seqf;
        private HashFile hashf;
        private string hashfFile;
        private string seqfFile;

        private bool isClosed = false;

        public bool IsClosed { get => isClosed; set => isClosed = value; }

        public NSmartDb(String file, string indexFile)
        {
           // if (indexFile == null) indexFile = "idx_" + file;
            hashfFile = file;
            seqfFile = indexFile;
            Open();
        }

        public void Open()
        {
            if (isClosed)
            {
                seqf = new SequenceFile(hashfFile);
                hashf = new HashFile(seqfFile);
                isClosed = false;
            }
        }

        public void Insert(long key, string value)
        {
            byte[] keyBytesbytes = BitConverter.GetBytes(key);
            if (hashf.Get(keyBytesbytes) != null) throw new Exception($"cant insert because hashfile has this item:{key}");
            byte[] valBytes = ASCIIEncoding.ASCII.GetBytes(value);
            //1.插入hash文件
            hashf.Put(keyBytesbytes, valBytes);
            //2.插入索引
            seqf.Add(key);
        }

        public void Update(long key, string value)
        {
            byte[] keyBytesbytes = BitConverter.GetBytes(key);
            if (hashf.Get(keyBytesbytes) == null) throw new Exception($"cant update because hashfile hasn't this item:{key}");
            byte[] valBytes = ASCIIEncoding.ASCII.GetBytes(value);
            //1.插入hash文件
            hashf.Put(keyBytesbytes, valBytes);
        }

        public string[] Select(int startIndex, int length)
        {
            string[] strs = new string[length];
            //获取索引
            var list = seqf.GetRange(startIndex, length);
            for (var i = 0; i < list.Count; i++)
            {
                strs[i] = ASCIIEncoding.ASCII.GetString
                    (hashf.Get
                         (BitConverter.GetBytes(list[i])
                     ?? new byte[] { 0 })
                     );
            }

            return strs;
            //输出
        }

        public void Delete(int index)
        {
            var key = seqf.Get(index);
            seqf.Delete(index);
            hashf.Remove(BitConverter.GetBytes(key));
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
    }
}
