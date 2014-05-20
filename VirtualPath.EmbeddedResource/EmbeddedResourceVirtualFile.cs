using System;
using System.IO;
using VirtualPath.Common;

namespace VirtualPath.EmbeddedResource
{
    public class EmbeddedResourceVirtualFile : AbstractVirtualFileBase
    {
        readonly EmbeddedResourceVirtualPathProvider Provider;
        readonly EmbeddedResource Embedded;

        public EmbeddedResourceVirtualFile(EmbeddedResourceVirtualPathProvider owningProvider, EmbeddedResourceVirtualDirectory directory, EmbeddedResource embedded)
            : base(owningProvider, directory)
        {
            if (embedded == null)
            {
                throw new ArgumentNullException("embedded");
            }

            Embedded = embedded;
            this.Provider = owningProvider;
        }

        public override string Name
        {
            get { return Path.GetFileName(Embedded.VirtualPath); }
        }

        public override System.DateTime LastModified
        {
            get { return DateTime.MinValue; }
        }

        public override Stream OpenWrite(WriteMode mode)
        {
            throw new NotImplementedException("EmbeddedResource is a readonly provider");
        }

        public override Stream OpenRead()
        {
            return Embedded.GetStream();
        }

        protected override IVirtualFile CopyBackingFileToDirectory(IVirtualDirectory directory, string name)
        {
            throw new System.NotImplementedException();
        }

        protected override IVirtualFile MoveBackingFileToDirectory(IVirtualDirectory directory, string name)
        {
            throw new System.NotImplementedException();
        }
    }
}