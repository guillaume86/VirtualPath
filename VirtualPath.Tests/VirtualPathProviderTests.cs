using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VirtualPath.FileSystem;
using VirtualPath.InMemory;

namespace VirtualPath.Tests
{
    public class VirtualPathProviderTests
    {
        public VirtualPathProviderTests()
        {
#if NCRUNCH
            //// Load types because NCrunch cause auto discovery to fail
            //var t1 = typeof(FtpVirtualPathProvider);
            //var t2 = typeof(SftpVirtualPathProvider);
            //var t3 = typeof(DropboxVirtualPathProvider);
#endif
        }

        [Test]
        public void Open_ReturnInMemoryProvider()
        {
            var provider = VirtualPathProvider.Open("memory");
            Assert.That(provider, Is.InstanceOf<InMemoryVirtualPathProvider>());
        }

        [Test]
        public void Open_ReturnFileSystemProvider_WithPathParameter()
        {
            var provider = VirtualPathProvider.Open("FS", "C:\\temp");
            Assert.That(provider, Is.InstanceOf<FileSystemVirtualPathProvider>());
        }

        [Test]
        public void Open_ReturnFileSystemProvider_WithAnonymousStringParameter()
        {
            var provider = VirtualPathProvider.Open("FS", new { rootPath = "C:\\temp" });
            Assert.That(provider, Is.InstanceOf<FileSystemVirtualPathProvider>());
        }

        [Test]
        public void Open_ReturnFileSystemProvider_WithConfigurationString()
        {
            var provider = VirtualPathProvider.Open("FS", "rootPath=C:\\temp");
            Assert.That(provider, Is.InstanceOf<FileSystemVirtualPathProvider>());
        }

        [Test]
        public void Open_ReturnFileSystemProvider_WithAnonymousDirectoryInfoParameter()
        {
            var provider = VirtualPathProvider.Open("FS", new { rootDirInfo = new DirectoryInfo("C:\\temp") });
            Assert.That(provider, Is.InstanceOf<FileSystemVirtualPathProvider>());
        }

        [Test]
        public void Open_ReturnProvider()
        {
            Assert.That(VirtualPathProvider.Open("Memory"), Is.InstanceOf<IVirtualPathProvider>());
            Assert.That(VirtualPathProvider.Open("FS", "C:\\temp"), Is.InstanceOf<IVirtualPathProvider>());
            //Assert.That(VirtualPathProvider.Open("SFTP", "username=test;password=test;host=127.0.0.1"), Is.InstanceOf<IVirtualPathProvider>());
            //Assert.That(VirtualPathProvider.Open("FTP", "username=test;password=test;host=127.0.0.1"), Is.InstanceOf<IVirtualPathProvider>());
            //Assert.That(VirtualPathProvider.Open("Dropbox", "apiKey=;apiSecret=;userToken=;userSecret=;sandbox=true"), Is.InstanceOf<IVirtualPathProvider>());
            //Assert.That(VirtualPathProvider.Open("GoogleDrive", ""), Is.InstanceOf<IVirtualPathProvider>());
        }
    }
}
