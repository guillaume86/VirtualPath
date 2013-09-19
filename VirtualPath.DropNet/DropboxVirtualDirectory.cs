using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropNet.Models;
using VirtualPath.Common;

namespace VirtualPath.DropNet
{
    public class DropboxVirtualDirectory : AbstractVirtualDirectoryBase
    {
        public DropboxVirtualDirectory(DropboxVirtualPathProvider owningProvider, DropboxVirtualDirectory parentDirectory, MetaData metaData)
            : base(owningProvider, parentDirectory)
        {
            _name = metaData.Name;
        }

        public DropboxVirtualDirectory(DropboxVirtualPathProvider owningProvider, DropboxVirtualDirectory parentDirectory, string name)
            : base(owningProvider, parentDirectory)
        {
            _name = name;
        }

        private DropboxVirtualPathProvider Provider
        {
            get { return ((DropboxVirtualPathProvider)VirtualPathProvider); }
        }

        internal MetaData MetaData
        {
            get
            {
                return Provider.GetMetadata(this.VirtualPath);
            }
        }

        private List<MetaData> Contents
        {
            get
            {
                return GetContents();
            }
        }

        private List<MetaData> GetContents()
        {
            if (MetaData.Contents == null)
            {
                Provider.GetMetadata(this.VirtualPath, withContents: true);
                if (MetaData.Contents == null)
                {
                    throw new InvalidOperationException("Contents should not be null");
                }
            }

            return MetaData.Contents;
        }

        public override DateTime LastModified
        {
            get { return MetaData.ModifiedDate; }
        }

        public override IEnumerable<IVirtualFile> Files
        {
            get { return this.OfType<IVirtualFile>(); }
        }

        public override IEnumerable<IVirtualDirectory> Directories
        {
            get { return this.OfType<IVirtualDirectory>(); }
        }

        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        public override IEnumerator<IVirtualNode> GetEnumerator()
        {
            return Contents
                .Where(c => !c.Is_Deleted)
                .Select(c => c.Is_Dir
                    ? (IVirtualNode)new DropboxVirtualDirectory(this.Provider, this, c)
                    : (IVirtualNode)new DropboxVirtualFile(this.Provider, this, c.Name))
                .GetEnumerator();
        }

        protected override IVirtualFile GetFileFromBackingDirectoryOrDefault(string fileName)
        {
            return Files.FirstOrDefault(f => ComparePath(f.Name, fileName));
        }

        protected bool ComparePath(string str1, string str2)
        {
            return String.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        protected override IVirtualDirectory GetDirectoryFromBackingDirectoryOrDefault(string directoryName)
        {
            return Directories.FirstOrDefault(f => ComparePath(f.Name,directoryName));
        }

        protected override void DeleteBackingDirectoryOrFile(string pathToken)
        {
            var node = this.FirstOrDefault(n => ComparePath(n.Name, pathToken));
            if (node == null) return;

            if (Provider.HasMetadata(this.VirtualPath, withContents: true))
            {
                var content = MetaData.Contents.First(md => String.Equals(md.Path, node.VirtualPath, StringComparison.OrdinalIgnoreCase));
                MetaData.Contents.Remove(content);
            }

            Provider.Delete(node.VirtualPath);
        }

        protected override IVirtualDirectory AddDirectoryToBackingDirectoryOrDefault(string name)
        {
            var virtualPath = Provider.CombineVirtualPath(MetaData.Path, name);
            var metaData = Provider.CreateDirectory(virtualPath);

            // Auto get Contents if not yet fetched?
            if (MetaData.Contents != null
                && !MetaData.Contents.Contains(metaData))
            {
                MetaData.Contents.Add(metaData);
            }

            return new DropboxVirtualDirectory(Provider, this, metaData);
        }

        protected override IVirtualFile AddFileToBackingDirectoryOrDefault(string fileName, byte[] contents)
        {
            var metaData = Provider.CreateFile(MetaData.Path, fileName, contents);
            Contents.Add(metaData);
            return new DropboxVirtualFile(Provider, this, metaData.Name);
        }

        protected override System.IO.Stream AddFileToBackingDirectoryOrDefault(string fileName)
        {
            return Provider.CreateFile(MetaData, fileName);
        }
    }
}
