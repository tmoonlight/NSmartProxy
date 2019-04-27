using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NSmartProxy.Database
{
    public class SequenceFile
    {
        private FileStream rf;
        private BinaryWriter bw;
        private BinaryReader br;
        private long tableSize = 1024L; //1k
        private const int SEQUENCE_ITEM_LENGTH = 8;
        public SequenceFile(String file)
        {

            rf = new FileStream(file, FileMode.OpenOrCreate);
            bw = new BinaryWriter(rf);
            br = new BinaryReader(rf);
            if (rf.Length == 0)
            {
                rf.SetLength(tableSize * SEQUENCE_ITEM_LENGTH);
                rf.Position = 0;
                bw.Write(0L);
            }
        }

        public void Add(long data)
        {
            rf.Position = 0;
            //1.获取长度
            long totallen = br.ReadInt64();
            rf.Position = 0;
            //2.更新长度
            bw.Write(totallen + 1);
            rf.Position = (totallen + 1) * SEQUENCE_ITEM_LENGTH;
            //3.写入值
            bw.Write(data);
        }

        public List<long> GetRange(int indexFrom, int length)
        {
            rf.Position = 0;
            //new 一个长度为p0的数组
            List<long> values = new List<long>(length);
            rf.Position = (indexFrom + 1) * SEQUENCE_ITEM_LENGTH;
            for (int i = 0; i < length; i++)
            {
                long p = br.ReadInt64();
                values.Add(p);
            }
            return values;
        }

        public long GetLength()
        {
            return Get(-1);
        }

        public long Get(int index)
        {
            rf.Position = (index + 1) * SEQUENCE_ITEM_LENGTH;
            return br.ReadInt64();
        }

        public void Delete(int index)
        {
            rf.Position = 0;
            long totallen = br.ReadInt64();
            if (index > totallen) throw new Exception($"can't delete when totallen={totallen} index={index}");
            rf.Position = 0;
            bw.Write(totallen - 1);
            //末尾值直接赋值给index的位置，末尾值置0
            rf.Position = (totallen - 1 + 1) * SEQUENCE_ITEM_LENGTH;
            //用尾部的值替换被删除的值
            long lastval = br.ReadInt64();
            bw.Seek(-1 * SEQUENCE_ITEM_LENGTH, SeekOrigin.Current);
            bw.Write(0L);
            rf.Position = (index + 1) * SEQUENCE_ITEM_LENGTH;
            bw.Write(lastval);
        }

        public void Close()
        {
            bw.Close();
            br.Close();
            rf.Close();
        }
    }
}
