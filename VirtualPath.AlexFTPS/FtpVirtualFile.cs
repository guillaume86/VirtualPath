using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualPath.Common;

namespace VirtualPath.AlexFTPS
{
    public class FtpVirtualFile : AbstractVirtualFileBase
    {
        private FtpVirtualPathProvider Provider;

        public FtpVirtualFile(FtpVirtualPathProvider owningProvider, FtpVirtualDirectory directory,
            string name = null, DateTime? lastModified = null)
            : base(owningProvider, directory)
        {
            Provider = owningProvider;
            _name = name;
            _lastModified = lastModified ?? DateTime.MinValue;
        }

        private DateTime _lastModified;
        public override DateTime LastModified
        {
            get { return _lastModified; }
        }

        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        public override System.IO.Stream OpenRead()
        {
            return Provider.OpenRead(VirtualPath);
        }

        public override System.IO.Stream OpenWrite(WriteMode mode)
        {
            return Provider.OpenWrite(VirtualPath, mode);
        }

        protected override IVirtualFile CopyBackingFileToDirectory(IVirtualDirectory directory, string name)
        {
            return directory.CopyFile(this, name);
        }

        protected override IVirtualFile MoveBackingFileToDirectory(IVirtualDirectory directory, string name)
        {
            if (directory is FtpVirtualDirectory)
            {
                var dir = (FtpVirtualDirectory)directory;
                if (dir.Provider == this.Provider)
                {
                    var currentDir = (FtpVirtualDirectory)this.Directory;
                    currentDir.RemoveFromContents(this.Name);
                    Provider.Rename(this.VirtualPath, Provider.CombineVirtualPath(dir.VirtualPath, name));
                    return new FtpVirtualFile(Provider, dir, name, DateTime.Now);
                }
            }

            var newFile = directory.CopyFile(this, name);
            this.Delete();
            return newFile;
        }
    }
}
