using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3.IO;
using VirtualPath.Common;

namespace VirtualPath.AmazonS3
{
    public class S3VirtualDirectory : AbstractVirtualDirectoryBase
    {
        protected S3VirtualPathProvider Provider;

        protected S3DirectoryInfo BackingDirInfo;

        public override IEnumerable<IVirtualFile> Files
        {
            get
            {
                return BackingDirInfo.GetFiles()
                    .Select(fInfo => new S3VirtualFile(VirtualPathProvider, this, fInfo));
            }
        }

        public override IEnumerable<IVirtualDirectory> Directories
        {
            get
            {
                return BackingDirInfo.GetDirectories()
                    .Select(dInfo => new S3VirtualDirectory(Provider, this, dInfo));
            }
        }

        //protected override string GetVirtualPathToRoot()
        //{
        //    if (IsRoot)
        //        return String.Empty;

        //    return GetPathToRoot(VirtualPathProvider.VirtualPathSeparator, p => p.VirtualPath);
        //}

        //protected override string GetPathToRoot(string separator, Func<IVirtualDirectory, string> pathSel)
        //{
        //    var parentPath = ParentDirectory != null ? pathSel(ParentDirectory) : string.Empty;

        //    if (parentPath == string.Empty)
        //    {
        //        return Name;
        //    }
        //    else
        //    {
        //        return string.Concat(parentPath, separator, Name);
        //    }
        //}

        public override string Name
        {
            get { return BackingDirInfo.Name; }
        }

        public override DateTime LastModified
        {
            get { return BackingDirInfo.LastWriteTime; }
        }

        public override string RealPath
        {
            get { return BackingDirInfo.FullName; }
        }

        public S3VirtualDirectory(S3VirtualPathProvider owningProvider, IVirtualDirectory parentDirectory, S3DirectoryInfo dInfo)
            : base(owningProvider, parentDirectory)
        {
            Provider = owningProvider;
            this.BackingDirInfo = dInfo;
        }

        public override IEnumerator<IVirtualNode> GetEnumerator()
        {
            return this.Directories.Cast<IVirtualNode>()
                .Union(this.Files.Cast<IVirtualNode>())
                .GetEnumerator();
        }

        protected override IVirtualFile GetFileFromBackingDirectoryOrDefault(string fName)
        {
            var fInfo = EnumerateFiles(fName).FirstOrDefault();

            return fInfo != null
                ? new S3VirtualFile(VirtualPathProvider, this, fInfo)
                : null;
        }

        protected override IEnumerable<IVirtualFile> GetMatchingFilesInDir(string globPattern)
        {
            var matchingFilesInBackingDir = EnumerateFiles(globPattern)
                .Select(fInfo => (IVirtualFile)new S3VirtualFile(VirtualPathProvider, this, fInfo));
            
            return matchingFilesInBackingDir;
        }

        protected override IVirtualDirectory GetDirectoryFromBackingDirectoryOrDefault(string dName)
        {
            var dInfo = EnumerateDirectories(dName)
                .FirstOrDefault();

            return dInfo != null
                ? new S3VirtualDirectory(Provider, this, dInfo)
                : null;
        }

        public IEnumerable<S3FileInfo> EnumerateFiles(string pattern)
        {
            return BackingDirInfo.GetFiles(pattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<S3DirectoryInfo> EnumerateDirectories(string dirName)
        {
            return BackingDirInfo.GetDirectories(dirName, SearchOption.TopDirectoryOnly);
        }

        protected override void DeleteBackingDirectoryOrFile(string pathToken)
        {
            var fileOrDir = this.FirstOrDefault(f => f.Name == pathToken);
            if (fileOrDir == null)
            {
                return;
            }

            fileOrDir.Delete();
        }

        public override void Delete()
        {
            BackingDirInfo.Delete(true);
        }

        protected override IVirtualDirectory AddDirectoryToBackingDirectoryOrDefault(string name)
        {
            var dirInfo = BackingDirInfo.CreateSubdirectory(name);
            dirInfo.Create();
            return new S3VirtualDirectory(this.Provider, this, dirInfo);
        }

        protected override IVirtualFile AddFileToBackingDirectoryOrDefault(string fileName, byte[] contents)
        {
            if (Files.Any(f => f.Name == fileName))
            {
                throw new ArgumentException("File already exists.");
            }

            var virtualPath = Provider.CombineVirtualPath(this.VirtualPath, fileName);
            var fInfo = Provider.CreateFile(virtualPath, contents);
            return new S3VirtualFile(VirtualPathProvider, this, fInfo);
        }

        protected override Stream AddFileToBackingDirectoryOrDefault(string fileName)
        {
            if (Files.Any(f => f.Name == fileName))
            {
                throw new ArgumentException("File already exists.");
            }

            var virtualPath = Provider.CombineVirtualPath(this.VirtualPath, fileName);
            return Provider.CreateFile(virtualPath);
        }
    }
}
