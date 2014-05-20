using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VirtualPath.Tests;

namespace VirtualPath.EmbeddedResource.Tests
{
    public class IVirtualPathProviderTests : IVirtualPathProviderFixture
    {
        public IVirtualPathProviderTests()
            : base(() => new EmbeddedResourceVirtualPathProvider(Assembly.GetExecutingAssembly()))
        {

        }
    }
}