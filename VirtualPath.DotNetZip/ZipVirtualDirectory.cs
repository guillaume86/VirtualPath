using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using VirtualPath.Common;

namespace VirtualPath.DotNetZip
{
    public class ZipVirtualDirectory : AbstractVirtualDirectoryBase
    {
        private ZipVirtualPathProvider Provider;
        private ZipEntry Entry;

        public ZipVirtualDirectory(ZipVirtualPathProvider owningProvider, ZipEntry entry, ZipVirtualDirectory parentDir)
            : base (owningProvider, parentDir)
        {
            Provider = owningProvider;
            Entry = entry;
        }

        public override DateTime LastModified
        {
            get 
            { 
                return Entry != null
                    ? Entry.LastModified
                    : DateTime.MinValue; 
            }
        }

        public override IEnumerable<IVirtualFile> Files
        {
            get { return this.OfType<IVirtualFile>(); }
        }

        public override IEnumerable<IVirtualDirectory> Directories
        {
            get { return this.OfType<IVirtualDirectory>(); }
        }

        public override string Name
        {
            get 
            { 
                return Entry != null
                    ? Path.GetFileName(Entry.FileName.Substring(0, Entry.FileName.Length - 1)) // Remove trailing '/'
                    : null;
            }
        }

        public override IEnumerator<IVirtualNode> GetEnumerator()
        {
            return Provider.Zip.Entries
                .Where(BelongsToDirectory)
                .Select(e => e.IsDirectory
                    ? (IVirtualNode)new ZipVirtualDirectory(Provider, e, this)
                    : (IVirtualNode)new ZipVirtualFile(Provider, e, this))
                .GetEnumerator();
        }

        private bool BelongsToDirectory(ZipEntry e)
        {
            if (e == Entry)
            {
                return false;
            }

            if (this.VirtualPath != Provider.VirtualPathSeparator)
            {
                return e.FileName.StartsWith(this.VirtualPath.Substring(1) + Provider.VirtualPathSeparator);
            }
            else // Root directory
            {
                if (e.IsDirectory)
                {
                    return !e.FileName
                            .Substring(0, e.FileName.Length - 1)
                            .Contains(this.Provider.VirtualPathSeparator);
                }
                else
                {
                    return !e.FileName.Contains(this.Provider.VirtualPathSeparator);
                }
            }
        }

        protected override IVirtualFile GetFileFromBackingDirectoryOrDefault(string fileName)
        {
            return Files.FirstOrDefault(f => f.Name == fileName);
        }

        protected override IVirtualDirectory GetDirectoryFromBackingDirectoryOrDefault(string directoryName)
        {
            return Directories.FirstOrDefault(d => d.Name == directoryName);
        }

        protected override IVirtualFile AddFileToBackingDirectoryOrDefault(string fileName, byte[] contents)
        {
            var file = this.Files.FirstOrDefault(d => d.Name == fileName);
            if (file != null)
            {
                throw new ArgumentException("A file with the same name already exists");
            }

            var virtualPath = Provider.CombineVirtualPath(this.VirtualPath, fileName);
            var zipPath = GetZipPath(virtualPath, false);
            var newEntry = Provider.Zip.AddEntry(zipPath, contents);
            Provider.Save();
            return new ZipVirtualFile(Provider, newEntry, this);
        }

        protected override void DeleteBackingDirectoryOrFile(string pathToken)
        {
            var node = this.FirstOrDefault(e => e.Name == pathToken);
            if (node == null) return;

            if (node is IVirtualDirectory)
            {
                foreach (var subNode in ((IVirtualDirectory)node).ToArray())
                {
                    subNode.Delete();
                }
            }

            var zipPath = GetZipPath(node.VirtualPath, node is IVirtualDirectory);
            Provider.Zip.RemoveEntry(zipPath);
            Provider.Save();
        }

        private string GetZipPath(string virtualPath, bool isDirectory)
        {
            var zipPath = virtualPath.Substring(1);
            if (isDirectory)
            {
                zipPath += this.Provider.VirtualPathSeparator;
            }
            return zipPath;
        }

        protected override IVirtualDirectory AddDirectoryToBackingDirectoryOrDefault(string name)
        {
            var dir = this.Directories.FirstOrDefault(d => d.Name == name);
            if (dir != null)
            {
                return dir;
            }

            var virtualPath = Provider.CombineVirtualPath(this.VirtualPath, name);
            var newEntry = Provider.Zip.AddDirectoryByName(GetZipPath(virtualPath, true));
            Provider.Save();
            return new ZipVirtualDirectory(Provider, newEntry, this);
        }

        protected override System.IO.Stream AddFileToBackingDirectoryOrDefault(string fileName)
        {
            var file = AddFileToBackingDirectoryOrDefault(fileName, new byte[0]);
            return file.OpenWrite();
        }
    }
}
