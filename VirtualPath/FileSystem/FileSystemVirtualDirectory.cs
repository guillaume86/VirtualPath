using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VirtualPath;
using VirtualPath.Common;

namespace VirtualPath.FileSystem
{
    public class FileSystemVirtualDirectory : AbstractVirtualDirectoryBase
    {
        protected DirectoryInfo BackingDirInfo;

        public override IEnumerable<IVirtualFile> Files
        {
            get { return this.Where(n => !n.IsDirectory).Cast<IVirtualFile>(); }
        }

        public override IEnumerable<IVirtualDirectory> Directories
        {
            get { return this.Where(n => n.IsDirectory).Cast<IVirtualDirectory>(); }
        }

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

        public FileSystemVirtualDirectory(IVirtualPathProvider owningProvider, IVirtualDirectory parentDirectory, DirectoryInfo dInfo)
            : base(owningProvider, parentDirectory)
        {
            this.BackingDirInfo = dInfo;
        }

        public override IEnumerator<IVirtualNode> GetEnumerator()
        {
            var directoryNodes = BackingDirInfo.GetDirectories()
                .Select(dInfo => new FileSystemVirtualDirectory(VirtualPathProvider, this, dInfo));

            var fileNodes = BackingDirInfo.GetFiles()
                .Select(fInfo => new FileSystemVirtualFile(VirtualPathProvider, this, fInfo));

            return directoryNodes.Cast<IVirtualNode>()
                .Union<IVirtualNode>(fileNodes.Cast<IVirtualNode>())
                .GetEnumerator();
        }

        protected override IVirtualFile GetFileFromBackingDirectoryOrDefault(string fName)
        {
            var fInfo = EnumerateFiles(fName).FirstOrDefault();

            return fInfo != null
                ? new FileSystemVirtualFile(VirtualPathProvider, this, fInfo)
                : null;
        }

        protected override IEnumerable<IVirtualFile> GetMatchingFilesInDir(string globPattern)
        {
            var matchingFilesInBackingDir = EnumerateFiles(globPattern)
                .Select(fInfo => (IVirtualFile)new FileSystemVirtualFile(VirtualPathProvider, this, fInfo));

            return matchingFilesInBackingDir;
        }

        protected override IVirtualDirectory GetDirectoryFromBackingDirectoryOrDefault(string dName)
        {
            var dInfo = EnumerateDirectories(dName)
                .FirstOrDefault();

            return dInfo != null
                ? new FileSystemVirtualDirectory(VirtualPathProvider, this, dInfo)
                : null;
        }

        public IEnumerable<FileInfo> EnumerateFiles(string pattern)
        {
            return BackingDirInfo.GetFiles(pattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<DirectoryInfo> EnumerateDirectories(string dirName)
        {
            return BackingDirInfo.GetDirectories(dirName, SearchOption.TopDirectoryOnly);
        }

        protected override IVirtualFile AddFileToBackingDirectoryOrDefault(string fileName, byte[] contents)
        {
            if (Files.Any(f => f.Name == fileName))
            {
                throw new ArgumentException("File already exists.");
            }

            var realFileName = String.Concat(this.RealPath, VirtualPathProvider.RealPathSeparator, fileName);
            File.WriteAllBytes(realFileName, contents);
            var fileInfo = new FileInfo(realFileName);
            return new FileSystemVirtualFile(VirtualPathProvider, this, fileInfo);
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
            var realDirName = String.Concat(this.RealPath, VirtualPathProvider.RealPathSeparator, name);
            var dirInfo = System.IO.Directory.CreateDirectory(realDirName);
            return new FileSystemVirtualDirectory(this.VirtualPathProvider, this, dirInfo);
        }

        protected override Stream AddFileToBackingDirectoryOrDefault(string fileName)
        {
            if (Files.Any(f => f.Name == fileName))
            {
                throw new ArgumentException("File already exists.");
            }

            var realFileName = String.Concat(this.RealPath, VirtualPathProvider.RealPathSeparator, fileName);
            return File.OpenWrite(realFileName);
        }
    }
}