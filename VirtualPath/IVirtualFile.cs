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

        IVirtualFile Copy(IVirtualDirectory destination);
        IVirtualFile Copy(IVirtualDirectory destination, string destFilename);
        IVirtualFile Copy(string destDirVirtualPath);
        IVirtualFile Copy(string destDirVirtualPath, string destFilename);

        IVirtualFile Move(IVirtualDirectory destination);
        IVirtualFile Move(IVirtualDirectory destination, string destFilename);
        IVirtualFile Move(string destDirVirtualPath);
        IVirtualFile Move(string destDirVirtualPath, string destFilaname);
    }
}