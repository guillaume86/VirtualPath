using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VirtualPath.Tests;

namespace VirtualPath.AmazonS3.Tests
{
    [Ignore]
    public class IVirtualPathProviderTests : IVirtualPathProviderFixture
    {
        public IVirtualPathProviderTests()
            : base(GetProvider)
        {

        }

        private static IVirtualPathProvider GetProvider()
        {
            var provider = new S3VirtualPathProvider(
                TestVariables.AmazonS3.AccessKeyId,
                TestVariables.AmazonS3.SecretAccessKey,
                TestVariables.AmazonS3.Bucket);

            return provider;
        }
    }
}
