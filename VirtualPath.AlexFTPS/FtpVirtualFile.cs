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
    }
}
