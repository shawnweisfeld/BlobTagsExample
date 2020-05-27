using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2
{
    public static class MD5Helper
    {
        public static string MD5Hash(this string value)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(value));

            var hash = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hash.Append(hashBytes[i].ToString("x2"));
            }

            return hash.ToString();
        }
    }
}
