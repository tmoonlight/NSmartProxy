using System;
using System.Collections.Generic;

namespace NSmartProxy.Database
{
    public interface IDbOperator : IDisposable
    {
        IDbOperator Open();
        void Insert(long key, string value);
        void Insert(string key, string value);
        void Update(long key, string value);
        void UpdateByName(string userName,string newUserName, string value);
        List<string> Select(int startIndex, int length);
        string GetConfig(string userId);
        void SetConfig(string userId,string config);
        string Get(long key);
        string Get(string key);
        void Delete(int index);
        void DeleteHash(string key);
        long GetLength();
        void Close();
        bool Exist(string key);
        int GetCount();
    }
}