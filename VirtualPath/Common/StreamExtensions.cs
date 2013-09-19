using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VirtualPath.Common
{
    internal static class StreamExtensions
    {
        public static string ToMd5Hash(this Stream stream)
        {
            var hash = MD5.Create().ComputeHash(stream);
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
