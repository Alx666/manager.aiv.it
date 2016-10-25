using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    public static class StringExtensions
    {
        private static string s_hChars = "abcdefghijklmnoqqrstuvwxyz0123456789";
        private static Random s_hRand  = new System.Random();

        public static string Random(int iLength)
        {
            return new string(Enumerable.Repeat(s_hChars, iLength).Select(s => s[s_hRand.Next(s.Length)]).ToArray());
        }
    }
}