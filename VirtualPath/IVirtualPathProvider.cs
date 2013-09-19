using System;
using System.Collections.Generic;
using System.IO;

namespace VirtualPath
{
    public interface IVirtualPathProvider : IDisposable
    {
		IVirtualDirectory RootDirectory { get; }
        string VirtualPathSeparator { get; }
        string RealPathSeparator { get; }

        string CombineVirtualPath(string basePath, string relativePath);

        bool FileExists(string virtualPath);
        bool DirectoryExists(string virtualPath);

        IVirtualFile GetFile(string virtualPath);
        string GetFileHash(string virtualPath);
        string GetFileHash(IVirtualFile virtualFile);

        IVirtualDirectory GetDirectory(string virtualPath);

        IEnumerable<IVirtualFile> GetAllMatchingFiles(string globPattern, int maxDepth = Int32.MaxValue);

        Stream AddFile(string filePath);
        IVirtualFile AddFile(string filePath, byte[] contents);
        IVirtualFile AddFile(string filePath, string contents);

        IVirtualDirectory AddDirectory(string virtualPath);
    }
}
