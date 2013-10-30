using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet.Sftp;
using VirtualPath.Common;

namespace VirtualPath.SshNet
{
    public class SftpVirtualFile : AbstractVirtualFileBase
    {
        private SftpVirtualPathProvider Provider;
        private SftpFile _file;
        private Lazy<SftpFile> File;

        public SftpVirtualFile(SftpVirtualPathProvider owningProvider, IVirtualDirectory directory, string name, DateTime? lastModified)
            : base(owningProvider, directory)
        {
            this.Provider = owningProvider;
            this._name = name;
            this._lastModified = lastModified ?? DateTime.MinValue;
            this.File = new Lazy<SftpFile>(() => _file ?? Provider.GetSftpFile(this.VirtualPath));
        }

        public SftpVirtualFile(SftpVirtualPathProvider owningProvider, IVirtualDirectory directory, SftpFile file)
            : this(owningProvider, directory, file.Name, file.LastWriteTime)
        {
            this.Provider = owningProvider;
            this._file = file;
        }

        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        private DateTime _lastModified;
        public override DateTime LastModified
        {
            get { return _lastModified; }
        }

        public override System.IO.Stream OpenRead()
        {
            return Provider.GetStream(this.VirtualPath);
        }

        public override System.IO.Stream OpenWrite(WriteMode mode)
        {
            return Provider.OpenWrite(this.VirtualPath, mode);
        }

        protected override IVirtualFile CopyBackingFileToDirectory(IVirtualDirectory directory, string name)
        {
            return directory.CopyFile(this, name);
        }

        protected override IVirtualFile MoveBackingFileToDirectory(IVirtualDirectory directory, string name)
        {
            if (directory is SftpVirtualDirectory)
            {
                var dir = (SftpVirtualDirectory)directory;
                if (dir.Provider == this.Provider)
                {
                    ((SftpVirtualDirectory)this.Directory).RemoveFromCache(this);
                    File.Value.MoveTo(Provider.CombineVirtualPath(directory.VirtualPath, name));
                    return new SftpVirtualFile(Provider, dir, name, DateTime.Now);
                }
            }

            var newFile = directory.CopyFile(this, name);
            this.Delete();
            return newFile;
        }
    }
}
