using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;

namespace NSmartProxy.Database
{
    public class LiteDbOperator : IDbOperator
    {
        public const string SUPER_VARIABLE_INDEX_ID = "$index_id$";

        private LiteDatabase liteDb;
        private string filePath;
        private bool isClosed = true;//默认未开启状态

        private LiteCollection<KV> liteCollection;
        //rvate 

        public LiteDbOperator(String file)
        {
            // if (indexFile == null) indexFile = "idx_" + file;
            filePath = file;
            Open();
        }

        public void Dispose()
        {
            liteDb.Dispose();
        }

        public IDbOperator Open()
        {
            if (isClosed)
            {
                liteDb = new LiteDatabase(filePath);
                liteCollection = liteDb.GetCollection<KV>("users");
            }

            return this;
        }

        public void Insert(long key, string value)
        {
            liteCollection.Insert(new KV(
                key.ToString(),
                value.Replace(SUPER_VARIABLE_INDEX_ID, GetAvailableKey(65535).ToString()))
            );
        }

        public void Insert(string key, string value)
        {
            liteCollection.Insert(new KV(
                key,
                value.Replace(SUPER_VARIABLE_INDEX_ID, GetAvailableKey(65535).ToString()))
            );
        }

        public void Update(long key, string value)
        {
            liteCollection.Update(new KV(key.ToString(), value));
        }

        public List<string> Select(int startIndex, int length)
        {
            return liteCollection.FindAll().Select(kv => kv.Value).ToList();
        }

        public string Get(long key)
        {
            return liteCollection.FindById(new BsonValue(key)).Value;
        }

        public string Get(string key)
        {
            return liteCollection.FindById(key).Value;
        }

        public void Delete(int index)
        {
            //liteCollection.Delete()
            //no implementation
        }

        public void DeleteHash(string key)
        {
            liteCollection.Delete(new BsonValue(key));
        }

        public long GetLength()
        {
            try
            {
                return liteCollection.Count();
            }
            catch (NullReferenceException ex)
            {
                return 0;
            }
        }


        public void Close()
        {
            if (!isClosed)
            {
                this.Dispose();
                isClosed = true;
            }
        }

        public bool Exist(string key)
        {
            return liteCollection.Exists(kv => kv.Key == key);
        }

        public bool Exist(long key)
        {
            return liteCollection.Exists(kv => kv.Key == key.ToString());
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
    }

    public class KV
    {
        public KV()
        {
        }

        public KV(string k, string val)
        {
            Key = k;
            Value = val;
        }

        [BsonId]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
