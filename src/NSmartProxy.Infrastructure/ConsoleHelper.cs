using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace NSmartProxy.Infrastructure
{
    public static class ConsoleHelper
    {
        public static SecureString ReadPassword(string mask = "*")
        {
            var password = new SecureString();
            while (true)
            {
                var i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (i.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.RemoveAt(password.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    password.AppendChar(i.KeyChar);
                    Console.Write(mask);
                }
            }
            password.MakeReadOnly();
            return password;
        }

        private static string RestoreSecureString(this SecureString value)
        {
            var valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

    }
}
