using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VirtualPath.Common
{
    public static class StreamExtensions
    {
        public static byte[] ReadStreamToEnd(Stream stream)
        {
            var bytes = new List<byte>();
            var buffer = new byte[4096]; // 4 ko
            var readCount = 0;
            while ((readCount = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                bytes.AddRange(buffer.Take(readCount));
            }
            return bytes.ToArray();
        }

        internal static byte[] ReadToEnd(this Stream stream)
        {
            return ReadStreamToEnd(stream);
        }

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
