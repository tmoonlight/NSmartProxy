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
        List<string> Select(int startIndex, int length);
        string Get(long key);
        string Get(string key);
        void Delete(int index);
        long GetLength();
        void Close();
        bool Exist(string key);
    }
}