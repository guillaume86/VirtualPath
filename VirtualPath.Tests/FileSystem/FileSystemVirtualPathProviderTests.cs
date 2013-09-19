using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VirtualPath.FileSystem;

namespace VirtualPath.Tests.FileSystem
{
    //[Ignore]
    public class FileSystemVirtualPathProviderTests
    {
        [Test]
        public void ShouldThrowExceptionWhenDirectoryIsNotProvided()
        {
            Assert.Throws<ArgumentNullException>(() => new FileSystemVirtualPathProvider(default(DirectoryInfo)));
        }

        [Test]
        public void ShouldThrowExceptionWhenDirectoryDoesNotExists()
        {
            Assert.Throws<ApplicationException>(() => new FileSystemVirtualPathProvider("C:\\NotExisting\\"));
        }
    }
}
