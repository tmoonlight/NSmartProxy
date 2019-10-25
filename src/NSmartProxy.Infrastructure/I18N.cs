using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace NSmartProxy.Infrastructure
{
    public class I18N
    {
        private const string BASE_PATH = "i18n";
        private static string originFullPath;
        private static string lFullPath;
        public static Dictionary<string, string> LMap;
        public static string L(string lStr)
        {
            if (LMap.ContainsKey(lStr))
            {
                return LMap[lStr];
            }
            else
            {
                LMap.Add(lStr, lStr);
                File.AppendAllLines(originFullPath, new string[] { lStr });
                return lStr;
            }
        }

        static I18N()
        {

            LMap = new Dictionary<string, string>();
            string originLFile = "ol.txt";
            //debug模式，提取代码中的多语言字段
            string entryPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + BASE_PATH + "\\";
            string langSign = System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag.ToLowerInvariant();
            originFullPath = entryPath + originLFile;
            

            if (langSign.StartsWith("zh"))
            {
                lFullPath = entryPath + "zh.txt";
            }
            else
            {
                lFullPath = entryPath + "en.txt";
            }

            if (!Directory.Exists(entryPath))
            {
                Directory.CreateDirectory(entryPath);
            }
            if (!File.Exists(originFullPath))
            {
                File.Create(originFullPath).Close();
            }
            if (!File.Exists(lFullPath))
            {
                File.Create(lFullPath).Close();
            }

            //读取所有多语言文件 TODO 先暂时只读取原始文件
            var lstrs = File.ReadAllLines(lFullPath);
            var ostrs = File.ReadAllLines(originFullPath);
            for (var i = 0; i < ostrs.Length; i++)
            {
                if (i > lstrs.Length - 1)
                {
                    LMap[ostrs[i]] = ostrs[i];
                    File.AppendAllLines(lFullPath, new string[] { ostrs[i] });//补齐多的行
                }
                else
                {
                    LMap[ostrs[i]] = lstrs[i];
                }
            }
        }
    }
}
