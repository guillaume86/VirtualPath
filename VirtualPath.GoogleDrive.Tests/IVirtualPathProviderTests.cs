using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VirtualPath.Tests;
using VirtualPath.GoogleDrive;

namespace VirtualPath.GoogleDrive.Tests
{
    //[Ignore]
    public class IVirtualPathProviderTests : IVirtualPathProviderFixture
    {
        public IVirtualPathProviderTests()
            : base(GetProvider)
        {

        }

        [Test]
        public void Test()
        {
            var provider = GetProvider();
        }

        private static DriveVirtualPathProvider GetProvider()
        {
            var provider = new DriveVirtualPathProvider(
                "VirtualPath.Tests",
                TestVariables.GoogleDrive.ClientId,
                TestVariables.GoogleDrive.ClientSecret,
                TestVariables.GoogleDrive.AuthToken);

            return provider;
        }
    }

    public class Sample
    {
        //[Test]
        //public void Test()
        //{
        //    var sample = new DriveCommandLineSample();
        //    sample.Main(TestVariables.GoogleDrive.ClientId, TestVariables.GoogleDrive.ClientSecret);
        //}
    }
}
