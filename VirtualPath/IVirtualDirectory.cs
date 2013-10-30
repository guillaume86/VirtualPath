using System;
using System.Collections.Generic;
using System.IO;

namespace VirtualPath
{
    public interface IVirtualDirectory : IVirtualNode, IEnumerable<IVirtualNode>
    {
        bool IsRoot { get; }
        IVirtualDirectory ParentDirectory { get; }

        IEnumerable<IVirtualFile> Files { get; }
        IEnumerable<IVirtualDirectory> Directories { get; }

        IVirtualFile GetFile(string virtualPath);
        IVirtualDirectory GetDirectory(string virtualPath);

        IEnumerable<IVirtualFile> GetAllMatchingFiles(string globPattern, int maxDepth = 1);

        Stream CreateFile(string virtualPath);
        IVirtualFile CreateFile(string virtualPath, byte[] contents);
        IVirtualFile CreateFile(string virtualPath, string contents);

        IVirtualDirectory CreateDirectory(string virtualPath);

        void Delete(string virtualPath);
    }
}
