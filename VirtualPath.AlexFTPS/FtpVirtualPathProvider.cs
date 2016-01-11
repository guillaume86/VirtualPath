using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    public class FtpVirtualPathProvider : AbstractVirtualPathProviderBase
    {
		protected string Host;
		protected string Username;
		protected string Password;
        protected bool IsConnected;
		protected int? Port;

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

        public override void Dispose()
        {
            base.Dispose();
            Client.Dispose();
            IsConnected = false;
        }

        private IVirtualDirectory _rootDirectory;
        public override IVirtualDirectory RootDirectory
        {
            get { return _rootDirectory = (_rootDirectory ?? new FtpVirtualDirectory(this, null)); }
        }

        public override string VirtualPathSeparator
        {
            get { return "/"; }
        }

        public override string RealPathSeparator
        {
            get { return "/"; }
        }

        internal IEnumerable<DirectoryListItem> ListContents(string virtualPath)
        {
            return ConnectedClient.GetDirectoryList(virtualPath);
        }

        internal void CreateDirectoryInternal(string virtualPath)
        {
            ConnectedClient.MakeDir(virtualPath);
        }

        internal void DeleteDirectory(string virtualPath)
        {
            ConnectedClient.RemoveDir(virtualPath);
        }

        internal System.IO.Stream CreateFileInternal(string fileName)
        {
            return ConnectedClient.PutFile(fileName);
        }

        internal void CreateFileInternal(string fileName, byte[] contents)
        {
            using (var stream = CreateFileInternal(fileName))
            {
                stream.Write(contents, 0, contents.Length);
            }
        }

        internal void DeleteFile(string virtualPath)
        {
            ConnectedClient.DeleteFile(virtualPath);
        }

        internal System.IO.Stream OpenRead(string virtualPath)
        {
            return ConnectedClient.GetFile(virtualPath);
        }

        internal System.IO.Stream OpenWrite(string virtualPath, byte[] originalData)
        {
            return new InMemory.InMemoryStream((data) => CreateFileInternal(virtualPath, data), originalData);
        }

        internal System.IO.Stream OpenWrite(string virtualPath, WriteMode mode)
        {
            if (mode == WriteMode.Truncate)
            {
                return ConnectedClient.PutFile(virtualPath);
            }
            else if (mode == WriteMode.Append)
            {
                return ConnectedClient.AppendFile(virtualPath);
            }
            else // if (mode == WriteMode.Overwrite)
            {
                var bytes = default(byte[]);
                using (var readStream = ConnectedClient.GetFile(virtualPath))
                {
                    bytes = StreamExtensions.ReadStreamToEnd(readStream);
                }
                var stream = new InMemory.InMemoryStream((data) => CreateFileInternal(virtualPath, data), bytes);
                return stream;
            }
        }

        internal void Rename(string virtualPathSrc, string virtualPathDst)
        {
            ConnectedClient.RenameFile(virtualPathSrc, virtualPathDst);
        }
    }
}
