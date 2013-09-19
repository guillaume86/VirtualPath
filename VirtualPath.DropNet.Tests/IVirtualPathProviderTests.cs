using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VirtualPath.DropNet;
using VirtualPath.Tests;

namespace VirtualPath.DropNet.Tests
{
    public class IVirtualPathProviderTests : IVirtualPathProviderFixture
    {
        public IVirtualPathProviderTests()
            : base(GetProvider)
        {

        }

        private static IVirtualPathProvider GetProvider()
        {
            Assert.That(TestVariables.Dropbox.ApiKey, Is.Not.EqualTo("XXXX"));

            var provider = new DropboxVirtualPathProvider(
                TestVariables.Dropbox.ApiKey,
                TestVariables.Dropbox.ApiSecret,
                TestVariables.Dropbox.UserToken,
                TestVariables.Dropbox.UserSecret,
                true);

            return provider;
        }
    }
}
