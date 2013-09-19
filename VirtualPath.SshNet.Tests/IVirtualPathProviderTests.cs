using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualPath.Tests;

namespace VirtualPath.SshNet.Tests
{
    public class IVirtualPathProviderTests : IVirtualPathProviderFixture
    {
        public IVirtualPathProviderTests()
            : base(() => new SftpVirtualPathProvider("127.0.0.1", "test", "test"))
        { }
    }
}
