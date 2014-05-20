using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualPath.Common;

namespace VirtualPath.EmbeddedResource
{
    public class EmbeddedResourceVirtualDirectory : AbstractVirtualDirectoryBase
    {
        private readonly EmbeddedResourceVirtualPathProvider Provider;
        private readonly string _name;

        public EmbeddedResourceVirtualDirectory(EmbeddedResourceVirtualPathProvider owningProvider, EmbeddedResourceVirtualDirectory parent, string name)
            : base(owningProvider, parent)
        {
            this.Provider = owningProvider;
            this._name = name;
        }

        public override DateTime LastModified
        {
            get { return DateTime.MinValue; }
        }

        public override IEnumerable<IVirtualFile> Files
        {
            get 
            {
                return Provider.GetResources(this.VirtualPath)
                    .Where(r => !r.VirtualPath.Substring(this.VirtualPath.Length + 1).Contains(Provider.VirtualPathSeparator)) // Exclude files in subdirectories
                    .Select(r => new EmbeddedResourceVirtualFile(Provider, this, r));
            }
        }

        public override IEnumerable<IVirtualDirectory> Directories
        {
            get 
            {
                return Provider.GetDirectories(Provider.GetResources(this.VirtualPath))
                    .Select(vpath => vpath.Split(Provider.VirtualPathSeparator[0]).Last())
                    .Select(dirName => new EmbeddedResourceVirtualDirectory(Provider, this, dirName));
            }
        }

        public override string Name
        {
            get { return _name; }
        }

        public override IEnumerator<IVirtualNode> GetEnumerator()
        {
            return Files
                .Concat(Directories.Cast<IVirtualNode>())
                .GetEnumerator();
        }

        protected override IVirtualFile GetFileFromBackingDirectoryOrDefault(string fileName)
        {
            return Files.FirstOrDefault(d => d.Name == fileName);
        }

        protected override IVirtualDirectory GetDirectoryFromBackingDirectoryOrDefault(string directoryName)
        {
            return Directories.FirstOrDefault(d => d.Name == directoryName);
        }

        protected override IVirtualFile AddFileToBackingDirectoryOrDefault(string fileName, byte[] contents)
        {
            throw new NotImplementedException("EmbeddedResource is a readonly provider");
        }

        protected override void DeleteBackingDirectoryOrFile(string pathToken)
        {
            throw new NotImplementedException("EmbeddedResource is a readonly provider");
        }

        protected override IVirtualDirectory AddDirectoryToBackingDirectoryOrDefault(string name)
        {
            throw new NotImplementedException("EmbeddedResource is a readonly provider");
        }

        protected override System.IO.Stream AddFileToBackingDirectoryOrDefault(string fileName)
        {
            throw new NotImplementedException("EmbeddedResource is a readonly provider");
        }
    }
}
