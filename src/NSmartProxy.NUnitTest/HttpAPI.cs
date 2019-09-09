using System;
using System.IO;
using NSmartProxy;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [Test]
        public void TestTail()
        {
            var strs = GetLogFileInfo(3);
            Assert.LessOrEqual(strs.Length, 3);

        }

        private string[] GetLogFileInfo(int lastLines)
        {
            string baseLogPath = "D:\\3000git\\NSmartProxy\\src\\NSmartProxy.ServerHost\\bin\\Debug\\netcoreapp2.2\\log";
            DirectoryInfo dir = new DirectoryInfo(baseLogPath);
            FileInfo[] files = dir.GetFiles("*.log*");
            DateTime recentWrite = DateTime.MinValue;
            FileInfo recentFile = null;

            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > recentWrite)
                {
                    recentWrite = file.LastWriteTime;
                    recentFile = file;
                }
            }

            using (var fs = File.OpenRead(recentFile.FullName))
            {
                var sr = new StreamReader(fs);
                return sr.Tail(lastLines);
            }
        }

        

    }
}