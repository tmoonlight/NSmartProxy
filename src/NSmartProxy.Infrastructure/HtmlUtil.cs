using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy
{
    public static class HtmlUtil
    {
        public static byte[] GetContent(string html)
        {
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
