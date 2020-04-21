using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        ConcurrentDictionary<string, string> keyCache = new ConcurrentDictionary<string, string>();//缓存机制，防止重复大量查数据库
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

        //TODO 如下update方法可能会把config覆盖掉，之后再考虑怎么做
        public void Update(long key, string value)
        {
            keyCache.TryRemove(key.ToString(), out _);
            liteCollection.Update(new KV(key.ToString(), value));
        }

        public void UpdateByName(string userName, string newUserName, string value)
        {
            //存在修改索引的操作，所以删除后增加
            keyCache.TryRemove(userName, out _);
            //liteCollection.
            //liteCollection.Update(userName,new KV(newUserName, value));
            liteCollection.Delete(userName);
            liteCollection.Insert(new KV(newUserName, value));
        }

        //public void UpdateByName(string userName, string value)
        //{
        //    keyCache.TryRemove(userName, out _);
        //    liteCollection.Update(new KV(userName, value));
        //}
        public int GetCount()
        {
            return liteCollection.Count();
        }

        public List<string> Select(int startIndex, int length)
        {
            return liteCollection.FindAll().Select(kv => kv.Value).ToList();
        }

        public string GetConfig(string userId)
        {
            var obj = liteCollection.FindById(userId);
            if (obj != null)
            {
                return obj.Config;
            }

            return null;
        }

        public void SetConfig(string userId, string config)
        {
            var obj = liteCollection.FindById(userId);
            obj.Config = config;
            liteCollection.Update(obj);
        }

        public string Get(long key)
        {
            return Get(key.ToString());
        }

        public string Get(string key)
        {
            if (keyCache.ContainsKey(key))
            {
                return keyCache[key];
            }
            else
            {
                var obj = liteCollection.FindById(key);
                if (obj != null)
                {
                    keyCache.TryAdd(key, obj.Value);
                    return obj.Value;
                }
            }
            return null;
        }

        public void Delete(int index)
        {
            //liteCollection.Delete()
            //no implementation
        }

        public void DeleteHash(string key)
        {
            liteCollection.Delete(new BsonValue(key));
            keyCache.TryRemove(key, out _);
        }

        public long GetLength()
        {
            try
            {
                return liteCollection.Count();
            }
            catch (NullReferenceException ex)
            {
                _ = ex;
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
            if (keyCache.ContainsKey(key))
            {
                return true;
            }
            else
            {
                var obj = liteCollection.FindById(key);
                if (obj != null)
                {
                    keyCache.TryAdd(key, obj.Value);
                    return true;
                }
            }
            return false;
        }

        public bool Exist(long key)
        {
            return Exist(key.ToString());
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
        public string Config { get; set; }//客户端配置后期指定
    }
}
