using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NSmartProxy.Data
{
    public enum Protocol : byte
    {
        TCP = 0x00,
        HTTP = 0x01, //同时代表HTTP/HTTPS
        //HTTPS = 0x02,
        UDP = 0x04
    }

    [Serializable]
    public class ClientApp : ICloneable
    {
        [JsonIgnore]
        public int AppId { get; set; }
        public string IP { get; set; }
        public int TargetServicePort { get; set; }
        public int ConsumerPort { get; set; }
        public bool IsCompress { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Protocol Protocol { get; set; }
        public string Host { get; set; }
        public string Description { get; set; }

        public object Clone()
        {

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(this, null))
            {
                return null;
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(stream);
            }
        }
    }
}
