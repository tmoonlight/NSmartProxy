namespace NSmartProxy.Database
{
    public interface IDbOperator
    {
        void Open();
        void Insert(long key, string value);
        void Update(long key, string value);
        string[] Select(int startIndex, int length);
        void Delete(int index);
        long GetLength();
        void Close();
        bool Exist(string key);
    }
}