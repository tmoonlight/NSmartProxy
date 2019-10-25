using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Data
{
    public enum ServerStatus : byte
    {
        UnknowndFailed = 0x00,
        Success = 0x01,
        AuthFailed = 0x03,
        UserBanned = 0x04
    }
}
