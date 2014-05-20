using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualPath.Common;

namespace VirtualPath
{
    public static class VirtualPathExtensions
    {
        public static byte[] ReadAllBytes(this IVirtualFile file)
        {
            var bytes = new List<byte>();
            using (var stream = file.OpenRead())
            {  
                return stream.ReadToEnd();
            }
        }

        public static IVirtualFile CopyFile(this IVirtualDirectory directory, IVirtualFile file, string name)
        {
            // TODO: use stream to stream copy (need AddFile out stream)
            var contents = file.ReadAllBytes();
            return directory.CreateFile(name, contents);
        }

        public static IVirtualFile CopyFile(this IVirtualDirectory directory, IVirtualFile file)
        {
            return directory.CopyFile(file, file.Name);
        }
    }
}
