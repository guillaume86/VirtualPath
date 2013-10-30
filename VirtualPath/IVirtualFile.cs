using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VirtualPath
{
    public interface IVirtualFile : IVirtualNode
    {
        string Extension { get; }
        string GetFileHash();

        Stream OpenRead();
        StreamReader OpenText();
        StreamReader OpenText(Encoding encoding);
        string ReadAllText();
        string ReadAllText(Encoding encoding);

        Stream OpenWrite();
        Stream OpenWrite(WriteMode mode);

        IVirtualFile CopyTo(IVirtualDirectory destination);
        IVirtualFile CopyTo(IVirtualDirectory destination, string destFilename);
        IVirtualFile CopyTo(string destDirVirtualPath);
        IVirtualFile CopyTo(string destDirVirtualPath, string destFilename);

        IVirtualFile MoveTo(IVirtualDirectory destination);
        IVirtualFile MoveTo(IVirtualDirectory destination, string destFilename);
        IVirtualFile MoveTo(string destDirVirtualPath);
        IVirtualFile MoveTo(string destDirVirtualPath, string destFilaname);
    }
}