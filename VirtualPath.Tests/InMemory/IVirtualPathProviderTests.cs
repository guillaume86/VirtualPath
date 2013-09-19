using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualPath.InMemory;

namespace VirtualPath.Tests.InMemory
{
    public class IVirtualPathProviderTests : IVirtualPathProviderFixture
    {
        public IVirtualPathProviderTests()
            : base(() => new InMemoryVirtualPathProvider())
        { }
    }
}
