using Newtonsoft.Json;
using NSmartProxy.Data;
using System.IO;

namespace NSmartProxy.Infrastructure
{
    public static class ConfigHelper
    {
        /// <summary>
        /// 读配置
        /// </summary>
        /// <returns></returns>
        public static Config ReadAllConfig(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                StreamReader sr = new StreamReader(fs);
                var str = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<Config>(str);
            }
        }

        /// <summary>
        /// 村配置
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Config SaveChanges(this Config config, string path)
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