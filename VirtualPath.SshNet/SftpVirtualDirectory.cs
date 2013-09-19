using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet.Sftp;
using VirtualPath.Common;

namespace VirtualPath.SshNet
{
    public class SftpVirtualDirectory : AbstractVirtualDirectoryBase
    {
        private readonly SftpVirtualPathProvider Provider;
        private SftpFile File;

        public SftpVirtualDirectory(SftpVirtualPathProvider owningProvider, IVirtualDirectory parentDirectory, 
            String name = null, DateTime? lastModified = null) 
            : base(owningProvider, parentDirectory)
        {
            this.Provider = owningProvider;
            _name = name;
            _lastModified = lastModified ?? DateTime.MinValue;
        }

        public SftpVirtualDirectory(SftpVirtualPathProvider owningProvider, IVirtualDirectory parentDirectory, SftpFile file)
            : this(owningProvider, parentDirectory, file.Name, file.LastWriteTime)
        {
            File = file;
        }

        private DateTime _lastModified;
        public override DateTime LastModified
        {
            get 
            {
                return _lastModified;
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

        private List<SftpFile> _contents;
        private IEnumerable<SftpFile> Contents
        {
            get
            {
                return _contents = (_contents ?? Provider.GetContents(this.VirtualPath).ToList());
            }
        }

        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        public override IEnumerator<IVirtualNode> GetEnumerator()
        {
            return Contents
                .Select(c => c.IsDirectory
                    ? (IVirtualNode)new SftpVirtualDirectory(Provider, this, c)
                    : (IVirtualNode)new SftpVirtualFile(Provider, this, c))
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
                var file = (IVirtualFile)node;
                Provider.DeleteFile(file.VirtualPath);
            }
            else if (node is IVirtualDirectory)
            {
                var dir = (IVirtualDirectory)node;
                foreach (var subNode in dir.ToArray())
                {
                    subNode.Delete();
                }
                Provider.DeleteDirectory(dir.VirtualPath);
            }

            if (this._contents != null)
            {
                this._contents.RemoveAll(c => c.Name == node.Name);
            }
        }

        protected override IVirtualDirectory AddDirectoryToBackingDirectoryOrDefault(string name)
        {
            var virtualPath = Provider.CombineVirtualPath(this.VirtualPath, name);
            var dir = GetDirectory(virtualPath);
            if (dir != null)
            {
                return dir;
            }

            Provider.AddDirectory(virtualPath);

            // TODO: add local collection and make Contents merge both
            this._contents = null;

            return new SftpVirtualDirectory(Provider, this, name);
        }

        protected override IVirtualFile AddFileToBackingDirectoryOrDefault(string fileName, byte[] contents)
        {
            if (Files.Any(f => f.Name == fileName))
            {
                throw new ArgumentException("File already exists.");
            }

            var virtualPath = Provider.CombineVirtualPath(this.VirtualPath, fileName);
            Provider.CreateFile(virtualPath, contents);

            // TODO: add local collection and make Contents merge both
            this._contents = null;
            return new SftpVirtualFile(Provider, this, fileName, DateTime.Now);
        }

        protected override System.IO.Stream AddFileToBackingDirectoryOrDefault(string fileName)
        {
            if (Files.Any(f => f.Name == fileName))
            {
                throw new ArgumentException("File already exists.");
            }

            var virtualPath = Provider.CombineVirtualPath(this.VirtualPath, fileName);
            this._contents = null;
            return Provider.CreateFile(virtualPath);
        }
    }
}
