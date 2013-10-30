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

        IVirtualDirectory GetDirectory(string virtualPath);

        IEnumerable<IVirtualFile> GetAllMatchingFiles(string globPattern, int maxDepth = 1);

        Stream CreateFile(string filePath);
        IVirtualFile CreateFile(string filePath, byte[] contents);
        IVirtualFile CreateFile(string filePath, string contents);

        IVirtualDirectory CreateDirectory(string virtualPath);
    }
}
