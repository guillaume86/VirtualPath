using System.IO;
using System.Text;

namespace VirtualPath
{
    public interface IVirtualFile : IVirtualNode
    {
        IVirtualPathProvider VirtualPathProvider { get; }

        string Extension { get; }

        string GetFileHash();

        Stream OpenRead();
        StreamReader OpenText();
        StreamReader OpenText(Encoding encoding);
        string ReadAllText();
        string ReadAllText(Encoding encoding);

        Stream OpenWrite();
        Stream OpenWrite(WriteMode mode);
    }
}
