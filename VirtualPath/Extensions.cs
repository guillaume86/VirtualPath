using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPath
{
    public static class Extensions
    {
        public static byte[] ReadStreamToEnd(Stream stream)
        {
            var bytes = new List<byte>();
            var nextByte = 0;
            while ((nextByte = stream.ReadByte()) >= 0)
            {
                bytes.Add((byte)nextByte);
            }
            return bytes.ToArray();
        }

        internal static byte[] ReadToEnd(this Stream stream)
        {
            return ReadStreamToEnd(stream);
        }

        public static byte[] ReadAllBytes(this IVirtualFile file)
        {
            var bytes = new List<byte>();
            using (var stream = file.OpenRead())
            {  
                return stream.ReadToEnd();
            }
        }

        public static IVirtualFile AddFile(this IVirtualDirectory directory, IVirtualFile file, string name)
        {
            var contents = file.ReadAllBytes();
            return directory.AddFile(name, contents);
        }

        public static IVirtualFile AddFile(this IVirtualDirectory directory, IVirtualFile file)
        {
            return directory.AddFile(file, file.Name);
        }
    }
}
