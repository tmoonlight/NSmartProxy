using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy
{
    public static class HtmlUtil
    {
        //简单html请求解析
        public static byte[] GetContent(string html)
        {
            //

            //StringBuilder http = new StringBuilder();

            //http.AppendLine("HTTP/1.0 200 OK");//方便起见 http1.0实现服务器。
            //http.AppendLine("Content-type:text/html");
            //http.AppendLine("Connection:close");


            //StringBuilder html = new StringBuilder();

            //html.AppendLine("<html>");
            //html.AppendLine("<head>");
            //html.AppendLine("<link rel=\"icon\" href=\"data:;base64,=\" >");
            //html.AppendLine("<title>TMoonlight</title>");
            //html.AppendLine("</head>");
            //html.AppendLine("<body>");
            //html.AppendLine("teet");
            //html.AppendLine("</body>");
            //html.AppendLine("</html>");



            return Encoding.UTF8.GetBytes(html.ToString());
        }

        private static byte[] PartternWord = System.Text.Encoding.ASCII.GetBytes("GET /welcome/");
        private static byte[] PartternPostWord = System.Text.Encoding.ASCII.GetBytes("POST /welcome/");

        //GET /welcome 
        private static bool CompareBytes(byte[] wholeBytes, byte[] partternWord)
        {
            for (int i = 0; i < partternWord.Length; i++)
            {
                if (wholeBytes[i] != partternWord[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
