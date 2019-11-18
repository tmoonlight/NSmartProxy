using System.Diagnostics;
using System.Dynamic;
using Newtonsoft.Json;
using NSmartProxy.Data;
using System.IO;

namespace NSmartProxy.Infrastructure
{
    public static class ConfigHelper
    {
        public static string AppSettingFullPath
        {
            get
            {
                var processModule = Process.GetCurrentProcess().MainModule;
                return Path.GetDirectoryName(processModule?.FileName)
                       + Path.DirectorySeparatorChar
                       + "appsettings.json";
            }
        }

        /// <summary>
        /// 读配置
        /// </summary>
        /// <returns></returns>
        public static T ReadAllConfig<T>(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                StreamReader sr = new StreamReader(fs);
                var str = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(str);
            }
        }

        /// <summary>
        /// 存配置
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static T SaveChanges<T>(this T config, string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs);
                JsonTextWriter jsonWriter = new JsonTextWriter(sw)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, config);

                sw.Close();
            }

            return config;
        }
    }
}