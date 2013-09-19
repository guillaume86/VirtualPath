using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using NUnit.Framework;
using VirtualPath.DotNetZip;
using VirtualPath.Tests;

namespace VirtualPath.DotNetZip.Tests
{
    public class IVirtualPathProviderTests : IVirtualPathProviderFixture
    {
        public IVirtualPathProviderTests()
            : base(() => new ZipVirtualPathProvider(new ZipFile()))
        {

        }
    }
}