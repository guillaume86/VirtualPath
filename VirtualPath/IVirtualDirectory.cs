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
        IVirtualFile GetFile(Stack<string> virtualPath);

        IVirtualDirectory GetDirectory(string virtualPath);
        IVirtualDirectory GetDirectory(Stack<string> virtualPath);

        IEnumerable<IVirtualFile> GetAllMatchingFiles(string globPattern, int maxDepth = Int32.MaxValue);

        Stream AddFile(string virtualPath);
        Stream AddFile(Stack<string> virtualPath);
        IVirtualFile AddFile(string virtualPath, byte[] contents);
        IVirtualFile AddFile(Stack<string> virtualPath, byte[] contents);
        IVirtualFile AddFile(string virtualPath, string contents);
        IVirtualFile AddFile(Stack<string> virtualPath, string contents);

        IVirtualDirectory AddDirectory(string virtualPath);
        IVirtualDirectory AddDirectory(Stack<string> virtualPath);

        void Delete(string virtualPath);
        void Delete(Stack<string> virtualPath);
    }
}
