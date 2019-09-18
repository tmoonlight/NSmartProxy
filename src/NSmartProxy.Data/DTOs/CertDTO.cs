using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Data.DTOs
{
    /// <summary>
    /// 证书DTO
    /// </summary>
    public class CertDTO
    {
        public int Port;
        public string CreateTime;
        public string ToTime;
        public string Hosts;
        public string Extensions;
    }
}
