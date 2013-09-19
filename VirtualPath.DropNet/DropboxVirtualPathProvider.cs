using DropNet;
using DropNet.Models;
using VirtualPath.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using VirtualPath.InMemory;

namespace VirtualPath.DropNet
{
    [Export("DropNet", typeof(IVirtualPathProvider))]
    [Export("Dropbox", typeof(IVirtualPathProvider))]
    public class DropboxVirtualPathProvider : AbstractVirtualPathProviderBase
    {
        private readonly DropNetClient Client;
        private readonly Dictionary<string, MetaData> MetaDataCache;

        public DropboxVirtualPathProvider(DropNetClient client)
            : base()
        {
            this.Client = client;
            this.MetaDataCache = new Dictionary<string, MetaData>(StringComparer.OrdinalIgnoreCase);
        }

        public DropboxVirtualPathProvider(string apiKey, string apiSecret, string userToken, string userSecret, bool sandbox)
            : this(CreateClient(apiKey, apiSecret, userToken, userSecret, sandbox))
        {

        }

        private static DropNetClient CreateClient(string apiKey, string apiSecret, string userToken, string userSecret, bool sandbox)
        {
            var _client = new DropNetClient(apiKey, apiSecret, userToken, userSecret);
            _client.UseSandbox = sandbox;
            return _client;
        }

        private IVirtualDirectory _rootDirectory;
        public override IVirtualDirectory RootDirectory
        {
            get { return _rootDirectory = (_rootDirectory ?? new DropboxVirtualDirectory(this, null, VirtualPathSeparator)); }
        }

        public override string VirtualPathSeparator
        {
            get { return "/"; }
        }

        public override string RealPathSeparator
        {
            get { return "/"; }
        }

        internal bool HasMetadata(string virtualPath, bool withContents = false)
        {
            var md = default(MetaData);
            if (MetaDataCache.TryGetValue(virtualPath, out md))
            {
                if (!withContents || md.Contents != null) return true;
            }
            return false;
        }

        internal MetaData GetMetadata(string virtualPath, bool withContents = false)
        {
            var md = default(MetaData);
            if (MetaDataCache.TryGetValue(virtualPath, out md))
            {
                if(!withContents || md.Contents != null) return md;
            }

            //Console.WriteLine("Fetch metadata for: " + virtualPath);
            md = Client.GetMetaData(virtualPath);
            //Console.WriteLine("Fetched " + md.Path);
            RefreshCache(md);
            return md;
        }

        private void RefreshCache(MetaData md)
        {
            if (md.Contents == null)
            {
                var oldMd = default(MetaData);
                if (MetaDataCache.TryGetValue(md.Path, out oldMd))
                {
                    md.Contents = oldMd.Contents;
                }
            }
            else
            {
                foreach (var content in md.Contents)
                {
                    RefreshCache(content);
                }
            }

            MetaDataCache[md.Path] = md;
        }

        internal byte[] GetBytes(string virtualPath)
        {
            return Client.GetFile(virtualPath);
        }

        internal System.IO.Stream OpenRead(string virtualPath)
        {
            var bytes = GetBytes(virtualPath);
            return new MemoryStream(bytes, false);
        }

        internal void Delete(string virtualPath)
        {
            Client.Delete(virtualPath);
            MetaDataCache.Remove(virtualPath);
        }

        internal MetaData CreateDirectory(string virtualPath)
        {
            var dir = (DropboxVirtualDirectory)GetDirectory(virtualPath);
            if (dir != null)
            {
                return dir.MetaData;
            }
            var metaData = Client.CreateFolder(virtualPath);
            if (metaData.Contents == null)
            {
                metaData.Contents = new List<MetaData>();
            }
            RefreshCache(metaData);
            return metaData;
        }

        internal MetaData CreateFile(string virtualPath, string fileName, byte[] contents)
        {
            var file = GetFile(CombineVirtualPath(virtualPath, fileName));
            if (file != null)
            {
                throw new ArgumentException("A file with the same name already exists");
            }
            var metaData = Client.UploadFile(virtualPath, fileName, contents);
            if (metaData.Contents == null)
            {
                metaData.Contents = new List<MetaData>();
            }
            RefreshCache(metaData);

            return metaData;
        }

        internal Stream CreateFile(MetaData dirMetaData, string fileName)
        {
            var virtualPath = dirMetaData.Path;
            var file = GetFile(CombineVirtualPath(virtualPath, fileName));
            if (file != null)
            {
                throw new ArgumentException("A file with the same name already exists");
            }

            return new InMemoryStream((contents) =>
            {
                var newFileMetaData = this.CreateFile(virtualPath, fileName, contents);
                if (dirMetaData.Contents != null)
                {
                    dirMetaData.Contents.Add(newFileMetaData);
                }
            });
        }

        internal Stream OpenWrite(string virtualPath, string fileName, WriteMode mode)
        {
            Action<byte[]> onClose = (data) => Client.UploadFile(virtualPath, fileName, data);

            if (mode == WriteMode.Truncate)
            {
                return new InMemory.InMemoryStream(onClose);
            }
            else
            {
                var bytes = GetBytes(CombineVirtualPath(virtualPath, fileName));
                var stream = new InMemory.InMemoryStream(onClose, bytes);
                if (mode == WriteMode.Append)
                {
                    stream.Seek(stream.Length, System.IO.SeekOrigin.Begin);
                }
                return stream;
            }
        }
    }
}