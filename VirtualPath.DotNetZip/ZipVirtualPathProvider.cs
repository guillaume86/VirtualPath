using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualPath.Common;
using Ionic.Zip;
using System.IO;
using System.ComponentModel.Composition;

namespace VirtualPath.DotNetZip
{
    [Export("Zip", typeof(IVirtualPathProvider))]
    [Export("DotNetZip", typeof(IVirtualPathProvider))]
    public class ZipVirtualPathProvider : AbstractVirtualPathProviderBase
    {
        private string FilePath;
        private IVirtualFile VirtualFile;
        
        public ZipVirtualPathProvider(ZipFile zipFile)
        {
            this.Zip = zipFile;
        }

        public ZipFile Zip { get; private set; }

        public ZipVirtualPathProvider(string path)
        {
            if (!File.Exists(path))
            {
                Zip = new ZipFile();
                Zip.Name = path;
            }
            else
            {
                Zip = ZipFile.Read(path);
            }
            FilePath = path;
        }

        public ZipVirtualPathProvider(IVirtualFile file)
        {
            VirtualFile = file;
            using (var stream = file.OpenRead())
            {
                Zip = ZipFile.Read(stream);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            Save();

            if (FilePath != null || VirtualFile != null)
            {
                Save();
                Zip.Dispose();
            }
        }

        internal void Save()
        {
            if (Zip.Name != null)
            {
                Zip.Save();
            }
            else if (VirtualFile != null)
            {
                using (var stream = VirtualFile.OpenWrite(WriteMode.Truncate))
                {
                    Zip.Save(stream);
                }
            }
        }

        public override IVirtualDirectory RootDirectory
        {
            get { return new ZipVirtualDirectory(this, null, null); }
        }

        public override string VirtualPathSeparator
        {
            get { return "/"; }
        }

        public override string RealPathSeparator
        {
            get { return "/"; }
        }
    }
}
