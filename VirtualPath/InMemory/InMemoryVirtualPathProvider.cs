using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VirtualPath;
using VirtualPath.Common;
using System.ComponentModel.Composition;
using System.Text;

namespace VirtualPath.InMemory
{
    [Export("Memory", typeof(IVirtualPathProvider))]
    public class InMemoryVirtualPathProvider : AbstractVirtualPathProviderBase
    {
        public InMemoryVirtualPathProvider()
            : base()
        {
            this.rootDirectory = new InMemoryVirtualDirectory(this);
        }

        public InMemoryVirtualDirectory rootDirectory;

        public override IVirtualDirectory RootDirectory
        {
            get { return rootDirectory; }
        }

        public override string VirtualPathSeparator
        {
            get { return "/"; }
        }

        public override string RealPathSeparator
        {
            get { return "/"; }
        }

        public override IVirtualFile GetFile(string virtualPath)
        {
            return rootDirectory.GetFile(virtualPath)
                ?? base.GetFile(virtualPath);
        }
    }

    public class InMemoryVirtualDirectory : AbstractVirtualDirectoryBase
    {
        public InMemoryVirtualDirectory(IVirtualPathProvider owningProvider)
            : this(owningProvider, null) { }

        public InMemoryVirtualDirectory(IVirtualPathProvider owningProvider, IVirtualDirectory parentDirectory)
            : base(owningProvider, parentDirectory)
        {
            this.files = new List<InMemoryVirtualFile>();
            this.dirs = new List<InMemoryVirtualDirectory>();
            this.DirLastModified = DateTime.MinValue;
        }

        public DateTime DirLastModified { get; set; }
        public override DateTime LastModified
        {
            get { return DirLastModified; }
        }

        public List<InMemoryVirtualFile> files;

        public override IEnumerable<IVirtualFile> Files
        {
            get { return files.Cast<IVirtualFile>(); }
        }

        public List<InMemoryVirtualDirectory> dirs;

        public override IEnumerable<IVirtualDirectory> Directories
        {
            get { return dirs.Cast<IVirtualDirectory>(); }
        }

        public string DirName { get; set; }
        public override string Name
        {
            get { return DirName; }
        }

        public override IEnumerator<IVirtualNode> GetEnumerator()
        {
            return Files.Cast<IVirtualNode>()
                .Union(Directories)
                .GetEnumerator();
        }

        protected override IVirtualFile GetFileFromBackingDirectoryOrDefault(string fileName)
        {
            return files.FirstOrDefault(x => x.Name == fileName);
        }

        protected override IEnumerable<IVirtualFile> GetMatchingFilesInDir(string globPattern)
        {
            var matchingFilesInBackingDir = EnumerateFiles(globPattern).Cast<IVirtualFile>();
            return matchingFilesInBackingDir;
        }

        protected override IVirtualDirectory GetDirectoryFromBackingDirectoryOrDefault(string directoryName)
        {
            return dirs.SingleOrDefault(d => d.Name == directoryName);
        }

        protected override IVirtualFile AddFileToBackingDirectoryOrDefault(string fileName, byte[] contents)
        {
            if (Files.Any(f => f.Name == fileName))
            {
                throw new ArgumentException("A file with the same name already exists.");
            }

            var newFile = new InMemoryVirtualFile(VirtualPathProvider, this)
            {
                FilePath = VirtualPathProvider.CombineVirtualPath(this.VirtualPath, fileName),
                FileName = fileName,
                ByteContents = contents
            };
            this.files.Add(newFile);
            return newFile;
        }

        protected override void DeleteBackingDirectoryOrFile(string pathToken)
        {
            var dir = this.dirs.FirstOrDefault(d => d.Name == pathToken);
            if (dir != null)
            {
                this.dirs.Remove(dir);
            }

            var file = this.files.FirstOrDefault(d => d.Name == pathToken);
            if (file != null)
            {
                this.files.Remove(file);
            }
        }

        protected override IVirtualDirectory AddDirectoryToBackingDirectoryOrDefault(string name)
        {
            var newDir = new InMemoryVirtualDirectory(VirtualPathProvider, this)
            {
                DirName = name,
            };
            this.dirs.Add(newDir);
            return newDir;
        }

        protected override Stream AddFileToBackingDirectoryOrDefault(string fileName)
        {
            if (Files.Any(f => f.Name == fileName))
            {
                throw new ArgumentException("A file with the same name already exists.");
            }

            var newFile = new InMemoryVirtualFile(VirtualPathProvider, this)
            {
                FilePath = VirtualPathProvider.CombineVirtualPath(this.VirtualPath, fileName),
                FileName = fileName,
                ByteContents = new byte[0],
            };
            var stream = new InMemoryStream((contents) => newFile.ByteContents = contents);
            
            this.files.Add(newFile);

            return stream;
        }
    }

    public class InMemoryVirtualFile : AbstractVirtualFileBase
    {
        public InMemoryVirtualFile(IVirtualPathProvider owningProvider, IVirtualDirectory directory)
            : base(owningProvider, directory)
        {
            this.FileLastModified = DateTime.MinValue;
        }

        public string FilePath { get; set; }

        public string FileName { get; set; }
        public override string Name
        {
            get { return FileName; }
        }

        public DateTime FileLastModified { get; set; }
        public override DateTime LastModified
        {
            get { return FileLastModified; }
        }

        public byte[] ByteContents { get; set; }

        public override Stream OpenRead()
        {
            return new MemoryStream(ByteContents ?? Encoding.UTF8.GetBytes(""));
        }

        public override Stream OpenWrite(WriteMode mode)
        {
            if (mode == WriteMode.Truncate)
            {
                return new InMemoryStream((data) =>
                {
                    ByteContents = data;
                });
            }
            else
            {
                var stream = new InMemoryStream((data) =>
                {
                    ByteContents = data;
                }, ByteContents ?? new byte[0]);

                if (mode == WriteMode.Append)
                {
                    stream.Seek(stream.Length, SeekOrigin.Begin);
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

    public class InMemoryStream : MemoryStream
    {
        private Action<byte[]> OnClose;
        public InMemoryStream(Action<byte[]> onClose)
        {
            this.OnClose = onClose;
        }

        public InMemoryStream(Action<byte[]> onClose, byte[] data)
            : this(onClose)
        {
            this.Write(data, 0, data.Length);
            this.Seek(0, SeekOrigin.Begin);
        }

        public override void Close()
        {
            OnClose(this.ToArray());
            base.Close();
        }
    }
}