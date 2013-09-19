using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VirtualPath.AlexFTPS;
using VirtualPath.Tests;

namespace VirtualPath.AlexFTPS.Tests
{
    public class IVirtualPathProviderTests : IVirtualPathProviderFixture
    {
        public IVirtualPathProviderTests()
            : base(() => new FtpVirtualPathProvider("127.0.0.1", "test", "test"))
        {

        }
    }
}