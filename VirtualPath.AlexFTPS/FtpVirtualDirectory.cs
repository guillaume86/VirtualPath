using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlexPilotti.FTPS.Common;
using VirtualPath.Common;

namespace VirtualPath.AlexFTPS
{
    public class FtpVirtualDirectory : AbstractVirtualDirectoryBase
    {
        private FtpVirtualPathProvider Provider;

        public FtpVirtualDirectory(FtpVirtualPathProvider owningProvider, FtpVirtualDirectory parentDirectory, 
            string name = null, DateTime? lastModified = null) : base(owningProvider, parentDirectory)
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

        public override IEnumerable<IVirtualFile> Files
        {
            get { return this.OfType<IVirtualFile>(); }
        }

        public override IEnumerable<IVirtualDirectory> Directories
        {
            get { return this.OfType<IVirtualDirectory>(); }
        }

        private List<DirectoryListItem> _contents;
        private IEnumerable<DirectoryListItem> Contents
        {
            get { return _contents = (_contents ?? GetContents().ToList()); }
        }

        private IEnumerable<DirectoryListItem> GetContents()
        {
            return Provider.ListContents(this.VirtualPath);
        }

        public override IEnumerator<IVirtualNode> GetEnumerator()
        {
            return Contents
                .Select(c => c.IsDirectory 
                    ? (IVirtualNode)new FtpVirtualDirectory(Provider, this, c.Name, c.CreationTime)
                    : (IVirtualNode)new FtpVirtualFile(Provider, this, c.Name, c.CreationTime))
                .GetEnumerator();
        }

        protected override IVirtualFile GetFileFromBackingDirectoryOrDefault(string fileName)
        {
            return Files.FirstOrDefault(f => f.Name == fileName);
        }

        protected override IVirtualDirectory GetDirectoryFromBackingDirectoryOrDefault(string directoryName)
        {
            return Directories.FirstOrDefault(d => d.Name == directoryName);
        }

        protected override void DeleteBackingDirectoryOrFile(string pathToken)
        {
            var node = this.FirstOrDefault(n => n.Name == pathToken);
            if (node == null) return;

            if (node is IVirtualFile)
            {
                Provider.DeleteFile(node.VirtualPath);
            }
            else if (node is IVirtualDirectory)
            {
                var dir = (IVirtualDirectory)node;
                foreach (var subNode in dir.ToArray())
                {
                    subNode.Delete();
                }
                Provider.DeleteDirectory(node.VirtualPath);
            }

            if (this._contents != null)
            {
                this._contents.RemoveAll(c => c.Name == node.Name);
            }
        }

        protected override IVirtualDirectory AddDirectoryToBackingDirectoryOrDefault(string name)
        {
            var dir = this.Directories.FirstOrDefault(d => d.Name == name);
            if(dir != null)
            {
                return dir;
            }

            var virtualPath = Provider.CombineVirtualPath(this.VirtualPath, name);
            Provider.CreateDirectory(virtualPath);

            if (this._contents != null)
            {
                this._contents.Add(new DirectoryListItem
                {
                    IsDirectory = true,
                    Name = name,
                    CreationTime = DateTime.Now,
                });
            }

            return new FtpVirtualDirectory(Provider, this, name);
        }

        protected override IVirtualFile AddFileToBackingDirectoryOrDefault(string fileName, byte[] contents)
        {
            var virtualPath = AddFileToContents(fileName);
            Provider.CreateFile(virtualPath, contents);
            return new FtpVirtualFile(Provider, this, fileName, DateTime.Now);
        }

        private string AddFileToContents(string fileName)
        {
            var file = this.Files.FirstOrDefault(d => d.Name == fileName);
            if (file != null)
            {
                throw new ArgumentException("A file with the same name already exists");
            }

            var virtualPath = Provider.CombineVirtualPath(this.VirtualPath, fileName);
            if (this._contents != null)
            {
                this._contents.Add(new DirectoryListItem
                {
                    IsDirectory = false,
                    Name = fileName,
                    CreationTime = DateTime.Now,
                });
            }
            return virtualPath;
        }

        protected override System.IO.Stream AddFileToBackingDirectoryOrDefault(string fileName)
        {
            var virtualPath = AddFileToContents(fileName);
            return Provider.CreateFile(virtualPath);
        }
    }
}
