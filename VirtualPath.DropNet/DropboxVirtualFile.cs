using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropNet.Models;
using VirtualPath.Common;

namespace VirtualPath.DropNet
{
    public class DropboxVirtualFile : AbstractVirtualFileBase
    {
        private DropboxVirtualPathProvider Provider
        {
            get { return ((DropboxVirtualPathProvider)VirtualPathProvider); }
        }

        private MetaData MetaData
        {
            get
            {
                return Provider.GetMetadata(this.VirtualPath);
            }
        }

        public DropboxVirtualFile(DropboxVirtualPathProvider owningProvider, DropboxVirtualDirectory directory, string name)
            : base(owningProvider, directory)
        {
            _name = name;
        }

        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        public override DateTime LastModified
        {
            get { return MetaData.ModifiedDate; }
        }

        public override System.IO.Stream OpenRead()
        {
            return Provider.OpenRead(this.VirtualPath);
        }

        public override System.IO.Stream OpenWrite(WriteMode mode)
        {
            return Provider.OpenWrite(this.Directory.VirtualPath, this.Name, mode);
        }
    }
}
