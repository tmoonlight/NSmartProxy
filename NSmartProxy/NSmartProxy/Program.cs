using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            Server srv = new Server();
            srv.Start();
            Console.Read();
        }
    }
}
