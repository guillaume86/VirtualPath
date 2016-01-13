using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AlexPilotti.FTPS.Client;
using AlexPilotti.FTPS.Common;
using VirtualPath.Common;

namespace VirtualPath.AlexFTPS
{
	[Export("AlexFTPS", typeof(IVirtualPathProvider))]
	[Export("FTP", typeof(IVirtualPathProvider))]
	public class FtpVirtualPathProvider : IVirtualPathProvider
	{
		protected string Host;
		protected string Username;
		protected string Password;
        protected bool IsConnected;
		protected int? Port;

		private readonly Dictionary<string, IVirtualNode> nodes;
		private readonly HashSet<string> listedPaths;

        public FTPSClient Client { get; protected set; }

        private FTPSClient ConnectedClient
        {
            get
            {
                if (!IsConnected)
                {
                    Connect();
                }
                return Client;
            }
        }

        public FtpVirtualPathProvider(FTPSClient client, string host, int? port, string username, string password)
        {
            this.Client = client;
            this.Host = host;
            this.Username = username;
            this.Password = password;
            this.Port = port;

			this.nodes = new Dictionary<string, IVirtualNode>();
			this.listedPaths = new HashSet<string>();
        }

        public FtpVirtualPathProvider(string host, string username, string password)
            : this(new FTPSClient(), host, null, username, password)
        {

        }

		public FtpVirtualPathProvider(string host, int port, string username, string password)
            : this(new FTPSClient(), host, port, username, password)
        {

        }

        protected virtual void Connect()
        {
			Client.Connect(
				this.Host,
				this.Port ?? 21,
				new NetworkCredential(this.Username, this.Password),
				ESSLSupportMode.ClearText,
				null, null, 0, 0, 0, null);

            IsConnected = true;
        }

        public void Dispose()
        {
            Client.Dispose();
            IsConnected = false;
        }

		public IVirtualDirectory RootDirectory
		{
			get { return this.GetDirectory("/"); }
		}

		public string VirtualPathSeparator
		{
			get { return "/"; }
		}

		public string RealPathSeparator
		{
			get { return "/"; }
		}

		public string CombineVirtualPath(string basePath, string relativePath)
		{
			basePath = basePath.TrimEnd('/');
			relativePath = relativePath.TrimStart('/');
			return String.Concat(basePath, VirtualPathSeparator, relativePath);
		}

		internal string GetParentPath(string path)
		{
			if (path == "/") return null;
			var parts = path.Split(new [] { this.VirtualPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
			return this.VirtualPathSeparator + String.Join(this.VirtualPathSeparator, parts.Take(parts.Count() - 1));
		}

		public bool FileExists(string virtualPath)
		{
			virtualPath = NormalizeAbsolutePath(virtualPath);
			return GetFile(virtualPath) != null;
		}

		public bool DirectoryExists(string virtualPath)
		{
			virtualPath = NormalizeAbsolutePath(virtualPath);
			return GetDirectory(virtualPath) != null;
		}

		public IVirtualFile GetFile(string virtualPath)
		{
			virtualPath = NormalizeAbsolutePath(virtualPath);
			var directoryPath = this.GetParentPath(virtualPath);
			this.FetchListing(directoryPath);

			var node = default(IVirtualNode);
			var exists = this.nodes.TryGetValue(virtualPath, out node);
			return (IVirtualFile)node;
		}

		public string GetFileHash(string virtualPath)
		{
			virtualPath = NormalizeAbsolutePath(virtualPath);
			return GetFile(virtualPath).GetFileHash();
		}

		public IVirtualDirectory GetDirectory(string virtualPath)
		{
			virtualPath = NormalizeAbsolutePath(virtualPath);
			var node = default(IVirtualNode);
			var exists = this.nodes.TryGetValue(virtualPath, out node);
			if (!exists)
			{
				var parentPath = this.GetParentPath(virtualPath);
				// We already listed the parent and it was not there
				if (listedPaths.Contains(parentPath)) return null;

				// We check the directory exists by listing its content (don't use parent because we may not have access)
				this.FetchListing(virtualPath);

				node = GetOrAddDirectory(virtualPath);
			}

			return (IVirtualDirectory)node;
		}

		private FtpVirtualDirectory GetOrAddDirectory(string virtualPath)
		{
			var node = new FtpVirtualDirectory(this, virtualPath);
			return this.AddOrGetNode(node);
		}

		private FtpVirtualFile GetOrAddFile(string virtualPath)
		{
			var node = new FtpVirtualFile(this, virtualPath);
			return this.AddOrGetNode(node);
		}

		private void FetchListing(string virtualPath)
		{
			if (this.listedPaths.Contains(virtualPath)) return;

			var contents = ConnectedClient.GetDirectoryList(virtualPath);
			foreach (var content in contents)
			{
				var path = this.CombineVirtualPath(virtualPath, content.Name);
				var node = content.IsDirectory
					? GetOrAddDirectory(path)
					: (IVirtualNode)GetOrAddFile(path);
			}

			this.listedPaths.Add(virtualPath);
		}

		private T AddOrGetNode<T>(T node) where T : IVirtualNode
		{
			var path = node.VirtualPath;
			var existingNode = default(IVirtualNode);
			var exists = this.nodes.TryGetValue(path, out existingNode);
			if (!exists)
			{
				this.nodes.Add(path, node);
				return (T)node;
			}
			return (T)existingNode;
		}

		public IEnumerable<IVirtualFile> GetAllMatchingFiles(string globPattern, int maxDepth = 1)
		{
            if (maxDepth == 0)
                yield break;

            foreach (var match in this.GetAllMatchingFiles("/", globPattern, maxDepth))
			{
				yield return match;
			}
		}

		internal IEnumerable<IVirtualFile> GetAllMatchingFiles(string path, string globPattern, int maxDepth = 1)
		{
			path = NormalizeAbsolutePath(path);
			this.FetchListing(path);

			var dir = this.GetDirectory(path);
			var matchesInDir = dir.Files.Where(f => f.Name.Glob(globPattern));
			foreach (var match in matchesInDir)
			{
				yield return match;
			}

			foreach (var childDir in dir.Directories)
			{
				var matchInChildDir = this.GetAllMatchingFiles(childDir.VirtualPath, globPattern, maxDepth - 1);
				foreach (var m in matchInChildDir)
					yield return m;
			}
		}

		public System.IO.Stream CreateFile(string filePath)
		{
			filePath = NormalizeAbsolutePath(filePath);

			if (this.nodes.ContainsKey(filePath)) throw new ArgumentException("File already exists", "filePath");

			var node = this.GetOrAddFile(filePath);

			return ConnectedClient.PutFile(filePath);
		}

		private string NormalizeAbsolutePath(string path)
		{
			path = path.TrimEnd('/');
			if (!path.StartsWith("/")) return "/" + path;
			return path;
		}

		public IVirtualFile CreateFile(string filePath, byte[] contents)
		{
			filePath = NormalizeAbsolutePath(filePath);
			using (var stream = CreateFile(filePath))
			{
				stream.Write(contents, 0, contents.Length);
			}

			return (IVirtualFile)this.nodes[filePath];
		}

		public IVirtualFile CreateFile(string filePath, string contents)
		{
			filePath = NormalizeAbsolutePath(filePath);
			var bytes = System.Text.Encoding.UTF8.GetBytes(contents);
			return this.CreateFile(filePath, bytes);
		}

		public IVirtualDirectory CreateDirectory(string virtualPath)
		{
			virtualPath = NormalizeAbsolutePath(virtualPath);
			var parentPath = this.GetParentPath(virtualPath);
			
			try
			{
				this.FetchListing(parentPath);
			}
			catch (FTPCommandException)
			{
				// Parent directory do not exists
				this.CreateDirectory(parentPath);
			}

			var node = default(IVirtualNode);
			var exists = this.nodes.TryGetValue(virtualPath, out node);
			if (node == null)
			{
				ConnectedClient.MakeDir(virtualPath);
				node = this.GetOrAddDirectory(virtualPath);
			}
			return (IVirtualDirectory)node;
		}

		internal System.IO.Stream OpenRead(string path)
		{
			path = NormalizeAbsolutePath(path);
			return ConnectedClient.GetFile(path);
		}

		internal Stream OpenWrite(string path, WriteMode mode)
		{
			path = NormalizeAbsolutePath(path);
			if (mode == WriteMode.Truncate)
			{
				return ConnectedClient.PutFile(path);
			}
			else if (mode == WriteMode.Append)
			{
				return ConnectedClient.AppendFile(path);
			}
			else if (mode == WriteMode.Overwrite)
			{
				var bytes = default(byte[]);
				using (var readStream = ConnectedClient.GetFile(path))
				{
					bytes = StreamExtensions.ReadStreamToEnd(readStream);
				}
				
				var stream = new InMemory.InMemoryStream((data) =>
				{
					using (var remoteStream = ConnectedClient.PutFile(path))
					{
						remoteStream.Write(data, 0, data.Length);
					}
				}, bytes);

				return stream;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		internal void DeleteFile(string path)
		{
			path = NormalizeAbsolutePath(path);
			ConnectedClient.DeleteFile(path);

			if (this.nodes.ContainsKey(path))
			{
				this.nodes.Remove(path);
			}
		}

		internal void DeleteDirectory(string path)
		{
			path = NormalizeAbsolutePath(path);

			var node = this.GetDirectory(path);

			if (node == null) return;

			var directories = node.Directories.ToArray();
			foreach (var subDir in directories)
			{
				this.DeleteDirectory(subDir.VirtualPath);
			}

			var files = node.Files.ToArray();
			foreach (var file in files)
			{
				this.DeleteFile(file.VirtualPath);
			}

			ConnectedClient.RemoveDir(path);

			if (this.nodes.ContainsKey(path))
			{
				this.nodes.Remove(path);
			}
		}

		internal IEnumerator<IVirtualNode> GetChildren(string path)
		{
			path = NormalizeAbsolutePath(path);
			this.FetchListing(path);
			return this.nodes.Values
				.Where(n => this.GetParentPath(n.VirtualPath) == path)
				.GetEnumerator();
		}

		internal IVirtualFile Move(FtpVirtualFile source, IVirtualDirectory destination, string destFilename)
		{
			if (destination.VirtualPathProvider == this)
			{
				var destPath = this.CombineVirtualPath(destination.VirtualPath, destFilename);
				this.ConnectedClient.RenameFile(source.VirtualPath, destPath);
				this.nodes.Remove(NormalizeAbsolutePath(source.VirtualPath));
				return this.GetOrAddFile(destPath);
			}
			else
			{
				var copy = source.CopyTo(destination, destFilename);
				source.Delete();
				return copy;
			}
		}
	}

	internal class FtpVirtualDirectory : IVirtualDirectory
	{
		private readonly FtpVirtualPathProvider provider;
		private readonly string path;
		private DateTime lastModified;

		public FtpVirtualDirectory(FtpVirtualPathProvider provider, string path)
		{
			this.provider = provider;
			this.path = path;
			this.lastModified = DateTime.MinValue;
		}

		public bool IsRoot
		{
			get { return this.path == "/"; }
		}

		public IVirtualDirectory ParentDirectory
		{
			get 
			{
				if (IsRoot) return null;
				return provider.GetDirectory(provider.GetParentPath(this.path)); 
			}
		}

		public IEnumerable<IVirtualFile> Files
		{
			get { return this.OfType<IVirtualFile>(); }
		}

		public IEnumerable<IVirtualDirectory> Directories
		{
			get { return this.OfType<IVirtualDirectory>(); }
		}

		public IVirtualFile GetFile(string virtualPath)
		{
			return provider.GetFile(provider.CombineVirtualPath(this.path, virtualPath));
		}

		public IVirtualDirectory GetDirectory(string virtualPath)
		{
			return provider.GetDirectory(provider.CombineVirtualPath(this.path, virtualPath));
		}

		public IEnumerable<IVirtualFile> GetAllMatchingFiles(string globPattern, int maxDepth = 1)
		{
			return this.provider.GetAllMatchingFiles(this.path, globPattern, maxDepth);
		}

		public System.IO.Stream CreateFile(string virtualPath)
		{
			return this.provider.CreateFile(provider.CombineVirtualPath(this.path, virtualPath));
		}

		public IVirtualFile CreateFile(string virtualPath, byte[] contents)
		{
			return this.provider.CreateFile(provider.CombineVirtualPath(this.path, virtualPath), contents);
		}

		public IVirtualFile CreateFile(string virtualPath, string contents)
		{
			return this.provider.CreateFile(provider.CombineVirtualPath(this.path, virtualPath), contents);
		}

		public IVirtualDirectory CreateDirectory(string virtualPath)
		{
			return provider.CreateDirectory(provider.CombineVirtualPath(this.path, virtualPath));
		}

		public void Delete(string virtualPath)
		{
			provider.DeleteDirectory(virtualPath);
		}

		public IVirtualPathProvider VirtualPathProvider
		{
			get { return provider; }
		}

		public IVirtualDirectory Directory
		{
			get { return this; }
		}

		public string Name
		{
			get { return Path.GetFileName(this.path); }
		}

		public string VirtualPath
		{
			get { return path; }
		}

		public string RealPath
		{
			get { return path; }
		}

		public bool IsDirectory
		{
			get { return true; }
		}

		public DateTime LastModified
		{
			get { return this.lastModified; }
		}

		public void Delete()
		{
			provider.DeleteDirectory(this.path);
		}

		public IEnumerator<IVirtualNode> GetEnumerator()
		{
			return provider.GetChildren(this.path);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}

	internal class FtpVirtualFile : IVirtualFile
	{
		private readonly FtpVirtualPathProvider provider;
		private string path;
		private DateTime lastModified;

		public FtpVirtualFile(FtpVirtualPathProvider provider, string path)
		{
			this.provider = provider;
			this.path = path;
			this.lastModified = DateTime.MinValue;
		}

		public string Extension
		{
			get { return Path.GetExtension(this.path); }
		}

		public string GetFileHash()
		{
			using (var stream = OpenRead())
			{
				return StreamExtensions.ToMd5Hash(stream);
			}
		}

		public System.IO.Stream OpenRead()
		{
			return this.provider.OpenRead(this.path);
		}

		public System.IO.StreamReader OpenText()
		{
			return OpenText(Encoding.UTF8);
		}

		public System.IO.StreamReader OpenText(Encoding encoding)
		{
			return new StreamReader(OpenRead(), encoding);
		}

		public string ReadAllText()
		{
			return ReadAllText(Encoding.UTF8);
		}

		public string ReadAllText(Encoding encoding)
		{
			using (var reader = OpenText(encoding))
			{
				var text = reader.ReadToEnd();
				return text;
			}
		}

		public System.IO.Stream OpenWrite()
		{
			return OpenWrite(WriteMode.Overwrite);
		}

		public System.IO.Stream OpenWrite(WriteMode mode)
		{
			return this.provider.OpenWrite(this.path, mode);
		}

		public IVirtualFile CopyTo(IVirtualDirectory destination)
		{
			return CopyTo(destination, this.Name);
		}

		public IVirtualFile CopyTo(IVirtualDirectory destination, string destFilename)
		{
			return destination.CopyFile(this, destFilename);
		}

		public IVirtualFile CopyTo(string destDirVirtualPath)
		{
			return CopyTo(destDirVirtualPath, this.Name);
		}

		public IVirtualFile CopyTo(string destDirVirtualPath, string destFilename)
		{
			var destination = provider.GetDirectory(destDirVirtualPath);
			return CopyTo(destination, destFilename);
		}

		public IVirtualFile MoveTo(IVirtualDirectory destination)
		{
			return MoveTo(destination, this.Name);
		}

		public IVirtualFile MoveTo(IVirtualDirectory destination, string destFilename)
		{
			return this.provider.Move(this, destination, destFilename);
		}

		public IVirtualFile MoveTo(string destDirVirtualPath)
		{
			return MoveTo(destDirVirtualPath, this.Name);
		}

		public IVirtualFile MoveTo(string destDirVirtualPath, string destFilaname)
		{
			var destination = provider.GetDirectory(destDirVirtualPath);
			return MoveTo(destination, destFilaname);
		}

		public IVirtualPathProvider VirtualPathProvider
		{
			get { return this.provider; }
		}

		public IVirtualDirectory Directory
		{
			get { return this.provider.GetDirectory(provider.GetParentPath(this.path)); }
		}

		public string Name
		{
			get { return Path.GetFileName(this.path); }
		}

		public string VirtualPath
		{
			get { return this.path; }
		}

		public string RealPath
		{
			get { return this.path; }
		}

		public bool IsDirectory
		{
			get { return false ; }
		}

		public DateTime LastModified
		{
			get { return this.lastModified; }
		}

		public void Delete()
		{
			this.provider.DeleteFile(this.path);
		}
	}
}
