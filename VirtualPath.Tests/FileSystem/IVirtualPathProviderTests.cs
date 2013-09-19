using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualPath.FileSystem;

namespace VirtualPath.Tests.FileSystem
{
    public class IVirtualPathProviderTests : IVirtualPathProviderFixture
    {
        public IVirtualPathProviderTests()
            : base(GetProvider)
        {

        }

        public static string RootPath = "C:\\Temp\\";

        public static IVirtualPathProvider GetProvider()
        {
            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
            }
            return new FileSystemVirtualPathProvider(RootPath);
        }

        public override void SetUp()
        {
            if (Directory.EnumerateFileSystemEntries(RootPath).Any())
            {
                throw new Exception("Can't start tests in non-empty directory.");
            }
            base.SetUp();
        }
    }
}
