using System;
using System.Collections.Generic;
using VirtualPath;

namespace VirtualPath.Common
{
    public abstract class AbstractVirtualPathProviderBase : IVirtualPathProvider, IDisposable
    {
        public abstract IVirtualDirectory RootDirectory { get; }
        public abstract string VirtualPathSeparator { get; }
        public abstract string RealPathSeparator { get; }

        protected AbstractVirtualPathProviderBase()
        {

        }

        public virtual string CombineVirtualPath(string basePath, string relativePath)
        {
            return String.Concat(basePath, VirtualPathSeparator, relativePath);
        }

        public virtual bool FileExists(string virtualPath)
        {
            return GetFile(virtualPath) != null;
        }

        public virtual bool DirectoryExists(string virtualPath)
        {
            return GetDirectory(virtualPath) != null;
        }

        public virtual IVirtualFile GetFile(string virtualPath)
        {
            return RootDirectory.GetFile(virtualPath);
        }

        public virtual string GetFileHash(string virtualPath)
        {
            var f = GetFile(virtualPath);
            return GetFileHash(f);
        }

        public virtual string GetFileHash(IVirtualFile virtualFile)
        {
            return virtualFile == null ? string.Empty : virtualFile.GetFileHash();
        }

        public virtual IVirtualDirectory GetDirectory(string virtualPath)
        {
            return RootDirectory.GetDirectory(virtualPath);
        }

        public virtual IEnumerable<IVirtualFile> GetAllMatchingFiles(string globPattern, int maxDepth = Int32.MaxValue)
        {
            return RootDirectory.GetAllMatchingFiles(globPattern, maxDepth);
        }

        public virtual bool IsSharedFile(IVirtualFile virtualFile)
        {
            return virtualFile.RealPath != null
                && virtualFile.RealPath.Contains(String.Format("{0}{1}", RealPathSeparator, "Shared"));
        }

        public virtual bool IsViewFile(IVirtualFile virtualFile)
        {
            return virtualFile.RealPath != null
                && virtualFile.RealPath.Contains(String.Format("{0}{1}", RealPathSeparator, "Views"));
        }

        public IVirtualFile CreateFile(string filePath, string contents)
        {
            return RootDirectory.CreateFile(filePath, contents);
        }

        public IVirtualFile CreateFile(string filePath, byte[] contents)
        {
            return RootDirectory.CreateFile(filePath, contents);
        }

        public IVirtualDirectory CreateDirectory(string virtualPath)
        {
            return RootDirectory.CreateDirectory(virtualPath);
        }

        public System.IO.Stream CreateFile(string filePath)
        {
            return RootDirectory.CreateFile(filePath);
        }

        public virtual void Dispose()
        {

        }
    }
}