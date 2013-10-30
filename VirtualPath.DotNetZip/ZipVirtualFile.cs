using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using VirtualPath.Common;
using VirtualPath.InMemory;

namespace VirtualPath.DotNetZip
{
    public class ZipVirtualFile : AbstractVirtualFileBase
    {
        private readonly ZipVirtualPathProvider Provider;
        private ZipEntry Entry;

        public ZipVirtualFile(ZipVirtualPathProvider owningProvider, ZipEntry entry, ZipVirtualDirectory directory)
            : base(owningProvider, directory)
        {
            this.Provider = owningProvider;
            this.Entry = entry;
        }

        public override string Name
        {
            get { return Path.GetFileName(Entry.FileName); }
        }

        public override DateTime LastModified
        {
            get { return Entry.LastModified; }
        }

        public override System.IO.Stream OpenRead()
        {
            if (Entry.Source == ZipEntrySource.Stream)
            {
                return Entry.InputStream;
            }
            else
            {
                return Entry.OpenReader();
            }
        }

        private void ReplaceBytes(byte[] data)
        {
            Provider.Zip.UpdateEntry(this.VirtualPath.Substring(1), data);
            Provider.Save();
            this.Entry = Provider.Zip.Entries.First(e => e.FileName == Entry.FileName);
        }

        public override Stream OpenWrite(WriteMode mode)
        {
            if (mode == WriteMode.Truncate)
            {
                // ! Workaround
                // if don't read before truncate, we get "a invalid stored block lengths" Exception from Ionic.Zip when reading after
                this.ReadAllBytes();
                return new InMemory.InMemoryStream(ReplaceBytes);
            }
            else
            {
                var bytes = this.ReadAllBytes();
                var stream = new InMemory.InMemoryStream(ReplaceBytes, bytes);
                if (mode == WriteMode.Append)
                {
                    stream.Seek(stream.Length, System.IO.SeekOrigin.Begin);
                }
                return stream;
            }
        }

        protected override IVirtualFile CopyBackingFileToDirectory(IVirtualDirectory directory, string name)
        {
            return directory.CopyFile(this, name);
        }

        protected override IVirtualFile MoveBackingFileToDirectory(IVirtualDirectory directory, string name)
        {
            var newFile = directory.CopyFile(this, name);
            this.Delete();
            return newFile;
        }
    }
}
