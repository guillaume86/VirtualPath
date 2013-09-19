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
        private SftpFile File;

        public SftpVirtualFile(SftpVirtualPathProvider owningProvider, IVirtualDirectory directory, string name, DateTime? lastModified)
            : base(owningProvider, directory)
        {
            this.Provider = owningProvider;
            this._name = name;
            this._lastModified = lastModified ?? DateTime.MinValue;
        }

        public SftpVirtualFile(SftpVirtualPathProvider owningProvider, IVirtualDirectory directory, SftpFile file)
            : this(owningProvider, directory, file.Name, file.LastWriteTime)
        {
            this.Provider = owningProvider;
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
    }
}
