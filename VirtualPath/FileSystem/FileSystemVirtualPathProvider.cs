using System;
using System.IO;
using VirtualPath;
using VirtualPath.Common;
using System.ComponentModel.Composition;

namespace VirtualPath.FileSystem
{
    [Export("FileSystem", typeof(IVirtualPathProvider))]
    [Export("FS", typeof(IVirtualPathProvider))]
    public class FileSystemVirtualPathProvider : AbstractVirtualPathProviderBase
    {
        protected DirectoryInfo RootDirInfo;
        protected FileSystemVirtualDirectory RootDir;

        public override IVirtualDirectory RootDirectory { get { return RootDir; } }
        public override String VirtualPathSeparator { get { return "/"; } }
        public override string RealPathSeparator { get { return Convert.ToString(Path.DirectorySeparatorChar); } }

        public FileSystemVirtualPathProvider(String rootPath)
            : this(new DirectoryInfo(rootPath))
        { }

        public FileSystemVirtualPathProvider(DirectoryInfo rootDirInfo)
            : base()
        {
            if (rootDirInfo == null)
                throw new ArgumentNullException("rootDirInfo");

            this.RootDirInfo = rootDirInfo;

            if (!RootDirInfo.Exists)
                throw new ApplicationException(
                    String.Format("RootDir '{0}' for virtual path does not exist", RootDirInfo.FullName));

            RootDir = new FileSystemVirtualDirectory(this, null, RootDirInfo);
        }
    }
}