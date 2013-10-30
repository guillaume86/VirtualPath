using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VirtualPath;

namespace VirtualPath.Common
{
    public abstract class AbstractVirtualFileBase : IVirtualFile
    {
        public IVirtualPathProvider VirtualPathProvider { get; set; }

        public string Extension
        {
            get { return Path.GetExtension(Name); }
        }

        public IVirtualDirectory Directory { get; set; }

        public abstract string Name { get; }
        public virtual string VirtualPath { get { return GetVirtualPathToRoot(); } }
        public virtual string RealPath { get { return GetRealPathToRoot(); } }
        public virtual bool IsDirectory { get { return false; } }
        public abstract DateTime LastModified { get; }

        protected AbstractVirtualFileBase(
            IVirtualPathProvider owningProvider, IVirtualDirectory directory)
        {
            if (owningProvider == null)
                throw new ArgumentNullException("owningProvider");

            if (directory == null)
                throw new ArgumentNullException("directory");

            this.VirtualPathProvider = owningProvider;
            this.Directory = directory;
        }

        public virtual string GetFileHash()
        {
            using (var stream = OpenRead())
            {
                return stream.ToMd5Hash();
            }
        }

        protected virtual String GetVirtualPathToRoot()
        {
            return GetPathToRoot(VirtualPathProvider.VirtualPathSeparator, p => p.VirtualPath);
        }

        protected virtual string GetRealPathToRoot()
        {
            return GetPathToRoot(VirtualPathProvider.RealPathSeparator, p => p.RealPath);
        }

        protected virtual string GetPathToRoot(string separator, Func<IVirtualDirectory, string> pathSel)
        {
            var parentPath = Directory != null ? pathSel(Directory) : string.Empty;
            if (parentPath == separator)
                parentPath = string.Empty;

            return string.Concat(parentPath, separator, Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as AbstractVirtualFileBase;
            if (other == null)
                return false;

            return other.VirtualPath == this.VirtualPath;
        }

        public override int GetHashCode()
        {
            return VirtualPath.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", RealPath, VirtualPath);
        }

        public virtual void Delete()
        {
            var parentDir = this.Directory;
            if (parentDir == null)
            {
                throw new ArgumentException("Delete root directory not allowed.");
            }
            parentDir.Delete(this.Name);
        }

        public abstract Stream OpenWrite(WriteMode mode);
        public virtual Stream OpenWrite()
        {
            return OpenWrite(WriteMode.Overwrite);
        }

        public virtual StreamReader OpenText(Encoding encoding)
        {
            return new StreamReader(OpenRead(), encoding);
        }

        public virtual string ReadAllText(Encoding encoding)
        {
            using (var reader = OpenText(encoding))
            {
                var text = reader.ReadToEnd();
                return text;
            }
        }

        public string ReadAllText()
        {
            return ReadAllText(Encoding.UTF8);
        }

        public abstract Stream OpenRead();

        public StreamReader OpenText()
        {
            return OpenText(Encoding.UTF8);
        }

        public virtual IVirtualFile Copy(IVirtualDirectory destination)
        {
            return Copy(destination, this.Name);
        }

        public virtual IVirtualFile Copy(IVirtualDirectory destination, string name)
        {
            return CopyBackingFileToDirectory(destination, name);
        }

        protected abstract IVirtualFile CopyBackingFileToDirectory(IVirtualDirectory directory, string name);

        public virtual IVirtualFile Copy(string destDirVirtualPath)
        {
            return Copy(destDirVirtualPath, this.Name);
        }

        public virtual IVirtualFile Copy(string destDirVirtualPath, string name)
        {
            var destination = VirtualPathProvider.GetDirectory(destDirVirtualPath);
            return Copy(destination, name);
        }

        public virtual IVirtualFile Copy(System.Collections.Generic.Stack<string> destDirVirtualPath)
        {
            var virtualPath = destDirVirtualPath.Aggregate((a, b) => VirtualPathProvider.CombineVirtualPath(a, b));
            return Copy(virtualPath);
        }

        public virtual IVirtualFile Copy(System.Collections.Generic.Stack<string> destDirVirtualPath, string name)
        {
            var virtualPath = destDirVirtualPath.Aggregate((a, b) => VirtualPathProvider.CombineVirtualPath(a, b));
            return Copy(virtualPath, name);
        }

        public virtual IVirtualFile Move(IVirtualDirectory destination)
        {
            return Move(destination, this.Name);
        }

        public virtual IVirtualFile Move(IVirtualDirectory destination, string name)
        {
            return MoveBackingFileToDirectory(destination, name);
        }

        protected abstract IVirtualFile MoveBackingFileToDirectory(IVirtualDirectory directory, string name);

        public virtual IVirtualFile Move(string destDirVirtualPath)
        {
            return Move(destDirVirtualPath, this.Name);
        }

        public virtual IVirtualFile Move(System.Collections.Generic.Stack<string> destDirVirtualPath)
        {
            var virtualPath = destDirVirtualPath.Aggregate((a, b) => VirtualPathProvider.CombineVirtualPath(a, b));
            return Move(virtualPath);
        }

        public virtual IVirtualFile Move(string destDirVirtualPath, string name)
        {
            var destination = VirtualPathProvider.GetDirectory(destDirVirtualPath);
            return Move(destination, name);
        }

        public virtual IVirtualFile Move(System.Collections.Generic.Stack<string> destDirVirtualPath, string name)
        {
            var virtualPath = destDirVirtualPath.Aggregate((a, b) => VirtualPathProvider.CombineVirtualPath(a, b));
            return Move(virtualPath, name);
        }
    }
}