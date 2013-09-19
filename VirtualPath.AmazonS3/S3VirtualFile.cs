using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.S3.IO;
using VirtualPath.Common;

namespace VirtualPath.AmazonS3
{
    public class S3VirtualFile : AbstractVirtualFileBase
    {
        protected S3FileInfo BackingFile;
        
        public override string Name
        {
            get { return BackingFile.Name; }
        }

        public override string RealPath
        {
            get { return BackingFile.FullName; }
        }

        public override DateTime LastModified
        {
            get { return BackingFile.LastWriteTime; }
        }

        public S3VirtualFile(IVirtualPathProvider owningProvider, IVirtualDirectory directory, S3FileInfo fInfo) 
            : base(owningProvider, directory)
        {
            this.BackingFile = fInfo;
        }

        public override Stream OpenRead()
        {
            return BackingFile.OpenRead();
        }

        public override void Delete()
        {
            BackingFile.Delete();
        }
    }
}
