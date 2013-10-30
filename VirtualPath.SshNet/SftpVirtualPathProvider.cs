using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using VirtualPath.Common;

namespace VirtualPath.SshNet
{
    [Export("SshNet", typeof(IVirtualPathProvider))]
    [Export("SFTP", typeof(IVirtualPathProvider))]
    public class SftpVirtualPathProvider : AbstractVirtualPathProviderBase
    {
        private readonly SftpClient Client;

        private SftpClient ConnectedClient
        {
            get
            {
                if (!Client.IsConnected)
                {
                    Client.Connect();
                }
                return Client;
            }
        }

        public SftpVirtualPathProvider(SftpClient client)
            : base()
        {
            this.Client = client;
        }

        public SftpVirtualPathProvider(string host, string username, string password)
            : this(GetClient(host, username, password)) { }

        public SftpVirtualPathProvider(string host, int port, string username, string password)
            : this(GetClient(host, username, password, port)) { }

        private static SftpClient GetClient(string host, string username, string password, int? port = null)
        {
            if (port != null)
            {
                return new SftpClient(host, port.Value, username, password);
            }
            else
            {
                return new SftpClient(host, username, password);
            }
        }

        private IVirtualDirectory _rootDirectory;
        public override IVirtualDirectory RootDirectory
        {
            get { return _rootDirectory = (_rootDirectory ?? new SftpVirtualDirectory(this, null)); }
        }

        public override string VirtualPathSeparator
        {
            get { return "/"; }
        }

        public override string RealPathSeparator
        {
            get { return "/"; }
        }

        internal IEnumerable<SftpFile> GetContents(string virtualPath)
        {
            return ConnectedClient.ListDirectory(virtualPath);
        }

        public override void Dispose()
        {
            Client.Dispose();
            base.Dispose();
        }

        internal void DeleteDirectory(string virtualPath)
        {
            ConnectedClient.DeleteDirectory(virtualPath);
        }

        internal void CreateDirectoryInternal(string virtualPath)
        {
            ConnectedClient.CreateDirectory(virtualPath);
        }

        internal Stream CreateFileInternal(string virtualPath)
        {
            return ConnectedClient.Create(virtualPath);
        }

        internal void CreateFileInternal(string virtualPath, byte[] contents)
        {
            using (var stream = CreateFileInternal(virtualPath))
            {
                stream.Write(contents, 0, contents.Length);
            }
        }

        internal void DeleteFile(string virtualPath)
        {
            ConnectedClient.DeleteFile(virtualPath);
        }

        internal Stream GetStream(string virtualPath)
        {
            var stream = new MemoryStream();
            ConnectedClient.DownloadFile(virtualPath, stream);
            stream.Position = 0;
            return stream;
        }

        internal Stream OpenWrite(string virtualPath)
        {
            using(var stream = GetStream(virtualPath))
            {
                var bytes = StreamExtensions.ReadStreamToEnd(stream);
                return new InMemory.InMemoryStream(
                    (data) => ConnectedClient.WriteAllBytes(virtualPath, data), 
                    bytes);
            }
        }

        internal Stream OpenWrite(string virtualPath, WriteMode mode)
        {
            Action<byte[]> onClose = (data) => ConnectedClient.WriteAllBytes(virtualPath, data);

            if (mode == WriteMode.Truncate)
            {
                ConnectedClient.DeleteFile(virtualPath);
                return new InMemory.InMemoryStream(onClose);
            }
            else if (mode == WriteMode.Overwrite)
            {
                return new InMemory.InMemoryStream(onClose);
            }
            else
            {
                var bytes = default(byte[]);
                using(var readStream = GetStream(virtualPath))
                {
                    bytes = StreamExtensions.ReadStreamToEnd(readStream);
                }
                var stream = new InMemory.InMemoryStream(onClose, bytes);
                stream.Seek(stream.Length, System.IO.SeekOrigin.Begin);
                return stream;
            }
        }

        internal SftpFile GetSftpFile(string virtualPath)
        {
            return ConnectedClient.Get(virtualPath);
        }
    }
}
