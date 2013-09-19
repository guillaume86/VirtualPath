using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VirtualPath.Tests;
using VirtualPath.SshNet;

namespace VirtualPath.SshNet.Tests
{
    public class SftpVirtualPathProviderTests
    {
        [Test]
        public void ShouldBeDisposable()
        {
            var provider = new SftpVirtualPathProvider("127.0.0.1", "test", "test");
            provider.Dispose();
        }

        [Test]
        public void ShouldHaveConstructorWithPort()
        {
            var provider = new SftpVirtualPathProvider("127.0.0.1", 22, "test", "test");
        }
    }
}
