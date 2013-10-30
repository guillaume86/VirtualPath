using NUnit.Framework;
using VirtualPath;
using VirtualPath.InMemory;
using VirtualPath.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPath.Tests
{
    public abstract class IVirtualPathProviderFixture
    {
        private Func<IVirtualPathProvider> ProviderFac;
        private IVirtualPathProvider Provider;

        public IVirtualPathProviderFixture(Func<IVirtualPathProvider> providerFac)
        {
            ProviderFac = providerFac;
        }

        [SetUp]
        public virtual void SetUp()
        {
            Provider = ProviderFac();
        }

        [TearDown]
        public virtual void TearDown()
        {
            if (Provider != null)
            {
                Clear();
                Provider.Dispose();
            }
        }

        private void Clear()
        {
            foreach (var node in Provider.RootDirectory.ToArray())
            {
                node.Delete();
            }
        }

        [Test]
        public void AddFile_CreateFile()
        {
            var name = "test.txt";
            var file = Provider.CreateFile(name, "");

            Assert.That(file, Is.Not.Null);
            Assert.That(file.IsDirectory, Is.False);
            Assert.That(file.Name, Is.EqualTo(name));
        }

        [Test]
        public void AddFile_AddFileToProvider()
        {
            var name = "Test.txt";
            var file = Provider.CreateFile(name, "");

            Assert.That(Provider.GetFile(name), Is.EqualTo(file));
        }

        [Test]
        public void AddFile_AddFileToRootDirectory()
        {
            var name = "test.txt";
            var file = Provider.CreateFile(name, "");

            Assert.That(Provider.RootDirectory.GetFile(name), Is.EqualTo(file));
        }

        [Test]
        public void AddFile_CreateFileWithStringContent()
        {
            var content = "blabla";
            var file = Provider.CreateFile("test.txt", content);
            Assert.That(file.ReadAllText(), Is.EqualTo(content));
        }

        [Test]
        public void AddFile_CreateFileWithBytesContent()
        {
            var content = new byte[] { 0x12, 0x10, (byte)1 };
            var file = Provider.CreateFile("test.txt", content);
            using (var stream = file.OpenRead())
            {
                // Don't use .Length to support non-searchable streams
                var bytes = new List<byte>();
                var next = 0;
                while ((next = stream.ReadByte()) > -1)
                {
                    bytes.Add((byte)next);
                }

                Assert.That(bytes, Is.EquivalentTo(content));
            }
        }

        [Test]
        public void CombineVirtualPath_ReturnValue()
        {
            var path = Provider.CombineVirtualPath("folder1", "folder2");
            Assert.That(path, Is.Not.Null);
        }

        [Test]
        public void CombineVirtualPath_ReturnCorrectPath()
        {
            var path1 = "folder1";
            var path2 = "folder2";

            var dir = Provider.CreateDirectory(path1).CreateDirectory(path2);
            var path = Provider.CombineVirtualPath(path1, path2);
            Assert.That(Provider.GetDirectory(path), Is.EqualTo(dir));
        }

        [Test]
        public void DirectoryExists_ReturnCorrectValue()
        {
            Provider.RootDirectory.CreateDirectory("Folder1");
            Assert.That(Provider.DirectoryExists("/Folder1"), Is.True);
            Assert.That(Provider.DirectoryExists("/xFolder1"), Is.False);
        }
        
        [Test]
        public void FileExists_ReturnCorrectValue()
        {
            Provider.RootDirectory.CreateFile("File1.ext", "");
            Assert.That(Provider.FileExists("/File1.ext"), Is.True);
            Assert.That(Provider.FileExists("/xFiles"), Is.False);
        }
        
        [Test]
        public void ReturnAllMatchingFiles_ReturnAllFilesInProvider()
        {
            Provider.RootDirectory.CreateFile("File1.ext", "");
            Provider.RootDirectory.CreateFile("File2.ext", "");
            Provider.RootDirectory.CreateFile("File3.ext", "");
            Provider.RootDirectory.CreateDirectory("Folder1");
            Provider.RootDirectory.CreateFile("Folder1/File1.ext", "");

            var files = Provider.GetAllMatchingFiles("*.ext", int.MaxValue);
            Assert.That(files.Select(d => d.VirtualPath), Is.EquivalentTo(new[]
            {
                "/File1.ext",
                "/File2.ext",
                "/File3.ext",
                "/Folder1/File1.ext",
            }));
        }

        [Test]
        [Timeout(100000)]
        public void GetDirectory_ReturnDirectory()
        {
            Provider.RootDirectory.CreateDirectory("Folder1");
            var dir = Provider.GetDirectory("Folder1");
            Assert.That(dir, Is.Not.Null);
            Assert.That(dir.IsDirectory, Is.True);
        }

        [Test]
        public void GetFile_ReturnFile()
        {
            Provider.RootDirectory.CreateFile("File1.ext", "");
            var file = Provider.GetFile("/File1.ext");
            Assert.That(file, Is.Not.Null);
            Assert.That(file.IsDirectory, Is.False);
        }

        [Test]
        public void GetFileHash_ReturnHash()
        {
            Provider.RootDirectory.CreateFile("File1.ext", "");
            var fileHash = Provider.GetFileHash("/File1.ext");
            Assert.That(fileHash, Is.Not.Null);
        }

        [Test]
        public void RootDirectory_IsNotNull()
        {
            Assert.That(Provider.RootDirectory, Is.Not.Null);
        }

        [Test]
        public void Directory_AddDirectory_CreateDirectory()
        {
            var dir = Provider.RootDirectory.CreateDirectory("Folder1");
            Assert.That(dir, Is.Not.Null);
        }

        [Test]
        public void Directory_AddDirectory_AddDirectoryToProvider()
        {
            var dir = Provider.RootDirectory.CreateDirectory("Folder1");
            Assert.That(Provider.GetDirectory("Folder1"), Is.EqualTo(dir));
        }

        [Test]
        public void Directory_AddDirectory_AddDirectoryToParent()
        {
            var dir = Provider.RootDirectory.CreateDirectory("Folder1");
            Assert.That(Provider.RootDirectory.GetDirectory("Folder1"), Is.EqualTo(dir));
        }

        [Test]
        public void Directory_AddDirectory_AddDirectoryToParent_WhenParentAlreadyFetched()
        {
            var dir = Provider.RootDirectory;
            var subDirs = dir.Directories.ToArray();
            var newDir = dir.CreateDirectory("Folder1");
            Assert.That(dir.GetDirectory("Folder1"), Is.EqualTo(newDir));
        }

        [Test]
        public void Directory_AddDirectory_AddDirectoryToParent_WhenCalledWithPath()
        {
            var dir = Provider.RootDirectory.CreateDirectory("/Folder1");
            Assert.That(Provider.RootDirectory.GetDirectory("/Folder1"), Is.EqualTo(dir));
            Assert.That(Provider.RootDirectory.GetDirectory("Folder1"), Is.EqualTo(dir));
        }

        [Test]
        public void Directory_AddDirectory_DoNotAddDirectoryToRoot_WhenCalledWithPath()
        {
            var dir = Provider.RootDirectory.CreateDirectory("/Folder1");
            var subDir = dir.CreateDirectory("/Folder2");

            Assert.That(dir.GetDirectory("/Folder2"), Is.EqualTo(subDir));
            Assert.That(Provider.GetDirectory("/Folder1/Folder2"), Is.EqualTo(subDir));
            Assert.That(Provider.RootDirectory.GetDirectory("/Folder2"), Is.Null);
        }

        [Test]
        public void Directory_AddDirectory_CreateIntermediatesDirectories_WhenCalledWithDeepPath()
        {
            var dir = Provider.RootDirectory.CreateDirectory("/Folder1/SubFolder1");
            Assert.That(Provider.RootDirectory.GetDirectory("/Folder1"), Is.Not.Null);
            Assert.That(Provider.RootDirectory.GetDirectory("/Folder1/SubFolder1"), Is.EqualTo(dir));
            Assert.That(Provider.RootDirectory.GetDirectory("/Folder1").GetDirectory("SubFolder1"), Is.EqualTo(dir));
        }
        
        [Test]
        public void Directory_AddDirectory_AddDirectoryToParent_WhenCalledWithDeepPath()
        {
            var dir = Provider.RootDirectory.CreateDirectory("/Folder1/SubFolder1");
            Assert.That(Provider.RootDirectory.GetDirectory("/Folder1/SubFolder1"), Is.EqualTo(dir));
            Assert.That(Provider.RootDirectory.GetDirectory("/Folder1").GetDirectory("SubFolder1"), Is.EqualTo(dir));
        }

        [Test]
        public void Directory_AddDirectory_DontThrowException_WhenAlreadyExists()
        {
            var dir1 = Provider.RootDirectory.CreateDirectory("/Folder1");
            var dir2 = Provider.RootDirectory.CreateDirectory("/Folder1");

            Assert.That(dir1, Is.EqualTo(dir2));
        }

        [Test]
        public void Directory_AddFile_ThrowArgumentException_WhenFileExists()
        {
            Provider.CreateFile("File1.ext", "");
            Assert.Throws<ArgumentException>(() => Provider.CreateFile("File1.ext", ""));
        }

        [Test]
        public void Directory_AddFile_CreateFile()
        {
            var name = "test.txt";
            var file = Provider.RootDirectory.CreateFile(name, "");

            Assert.That(file, Is.Not.Null);
            Assert.That(file.IsDirectory, Is.False);
            Assert.That(file.Name, Is.EqualTo(name));
        }

        [Test]
        public void Directory_AddFile_AddFileToProvider()
        {
            var name = "test.txt";
            var file = Provider.RootDirectory.CreateFile(name, "");

            Assert.That(Provider.GetFile(name), Is.EqualTo(file));
        }

        [Test]
        public void Directory_AddFile_AddFileToDirectory()
        {
            var name = "test.txt";
            var file = Provider.RootDirectory.CreateFile(name, "");

            Assert.That(Provider.RootDirectory.GetFile(name), Is.EqualTo(file));
        }

        [Test]
        public void Directory_AddFile_CreateFileWithStringContent()
        {
            var content = "blabla";
            var file = Provider.RootDirectory.CreateFile("test.txt", content);
            Assert.That(file.ReadAllText(), Is.EqualTo(content));
        }

        [Test]
        public void Directory_AddFile_CreateFileWithBytesContent()
        {
            var content = new byte[] { 0x12, 0x10, (byte)1 };
            var file = Provider.RootDirectory.CreateFile("test.txt", content);
            using (var stream = file.OpenRead())
            {
                // Don't use .Length to support non-searchable streams
                var bytes = new List<byte>();
                var next = 0;
                while ((next = stream.ReadByte()) > -1)
                {
                    bytes.Add((byte)next);
                }

                Assert.That(bytes, Is.EquivalentTo(content));
            }
        }

        [Test]
        public void Directory_AddFile_CreateFileWithStream()
        {
            var content = new byte[] { 0x12, 0x10, (byte)1 };
            using (var stream = Provider.RootDirectory.CreateFile("test.txt"))
            {
                stream.Write(content, 0, content.Length);
            }
            var file = Provider.GetFile("test.txt");
            using (var stream = file.OpenRead())
            {
                // Don't use .Length to support non-searchable streams
                var bytes = new List<byte>();
                var next = 0;
                while ((next = stream.ReadByte()) > -1)
                {
                    bytes.Add((byte)next);
                }

                Assert.That(bytes, Is.EquivalentTo(content));
            }
        }

        [Test]
        public void Directory_Delete_DeleteSubDirectory()
        {
            Provider.RootDirectory.CreateDirectory("Folder1");
            Provider.RootDirectory.Delete("Folder1");
            Assert.That(Provider.RootDirectory.GetDirectory("Folder1"), Is.Null);
            Assert.That(Provider.DirectoryExists("Folder1"), Is.False);
        }

        [Test]
        public void Directory_Delete_DeleteItSelf()
        {
            var dir = Provider.RootDirectory.CreateDirectory("Folder1");
            dir.Delete();
            Assert.That(Provider.RootDirectory.GetDirectory("Folder1"), Is.Null);
            Assert.That(Provider.DirectoryExists("Folder1"), Is.False);
        }

        [Test]
        public void Directory_Delete_DeleteSubDirectory_WhenCalledWithDeepPath()
        {
            Provider.RootDirectory.CreateDirectory("Folder1/Folder2/Folder3");
            Provider.RootDirectory.Delete("Folder1/Folder2/Folder3");
            var deletedDir = Provider.RootDirectory.GetDirectory("Folder1/Folder2/Folder3");
            Assert.That(deletedDir, Is.Null);
            Assert.That(Provider.RootDirectory.GetDirectory("Folder1/Folder2"), Is.Not.Null);
        }

        [Test]
        public void Directory_Delete_DoNotThrow_WhenDeleteNotExistingNode()
        {
            Provider.RootDirectory.Delete("Folder2");
            Provider.RootDirectory.Delete("File1.ext");
        }

        [Test]
        public void Directory_Name_IsCorrect()
        {
            Provider.RootDirectory.CreateDirectory("Folder1");
            var dir = Provider.RootDirectory.GetDirectory("Folder1");
            Assert.That(dir.Name, Is.EqualTo("Folder1"));
        }

        [Test]
        public void Directory_LastModified_DoNotThrow()
        {
            Provider.RootDirectory.CreateDirectory("Folder1");
            var dir = Provider.RootDirectory.GetDirectory("Folder1");
            var date = dir.LastModified;
        }

        [Test]
        public void Directory_ToString_IsNotNull()
        {
            Provider.RootDirectory.CreateDirectory("Folder1");
            var dir = Provider.RootDirectory.GetDirectory("Folder1");

            Assert.That(dir.ToString(), Is.Not.Null);
        }

        [Test]
        public void Directory_RealPath_IsNotNull()
        {
            Provider.RootDirectory.CreateDirectory("Folder1");
            var dir = Provider.RootDirectory.GetDirectory("Folder1");

            Assert.That(dir.RealPath, Is.Not.Null);
        }

        [Test]
        public void File_RealPath_IsNotNull()
        {
            Provider.RootDirectory.CreateFile("File1.ext", "");
            var file = Provider.RootDirectory.GetFile("File1.ext");
            Assert.That(file.RealPath, Is.Not.Null);
        }

        [Test]
        public void File_Extension_IsCorrect()
        {
            Provider.RootDirectory.CreateFile("File1.ext", "");
            var file = Provider.RootDirectory.GetFile("File1.ext");
            Assert.That(file.Extension, Is.EqualTo(".ext"));
        }

        [Test]
        public void File_LastModified()
        {
            Provider.RootDirectory.CreateFile("File1.ext", "");
            var file = Provider.RootDirectory.GetFile("File1.ext");
            var date = file.LastModified;
        }

        [Test]
        public void Node_Directory_IsNotNull()
        {
            Provider.RootDirectory.CreateFile("File1.ext", "");
            var file = Provider.RootDirectory.GetFile("File1.ext");
            Assert.That(file.Directory, Is.Not.Null);

            Assert.That(Provider.RootDirectory.Directories, Is.Not.Null);
        }

        [Test]
        public void Directory_Directories_ReturnCorrectValue()
        {
            Provider.RootDirectory.CreateDirectory("Folder1");
            Provider.RootDirectory.CreateDirectory("Folder2");

            var dirs = Provider.RootDirectory.Directories;

            Assert.That(dirs.Select(d => d.Name), Is.EquivalentTo(new[]
            {
                "Folder1",
                "Folder2"
            }));
        }

        [Test]
        public void Directory_Files_ReturnCorrectValue()
        {
            Provider.RootDirectory.CreateDirectory("Folder1");
            Provider.RootDirectory.CreateFile("File1.ext", "");
            Provider.RootDirectory.CreateFile("File2.ext", "");
            Provider.RootDirectory.CreateFile("File3.ext", "");

            var files = Provider.RootDirectory.Files;

            Assert.That(files.Select(d => d.Name), Is.EquivalentTo(new[]
            {
                "File1.ext",
                "File2.ext",
                "File3.ext",
            }));
        }

        [Test]
        public void File_OpenWrite()
        {
            var initial = new byte[] { 0x00, 0x01, 0x02 };
            var content = new byte[] { 0x03, 0x04 };
            var expected = new byte[] { 0x03, 0x04, 0x02 };
            var file = Provider.CreateFile("test.txt", initial);
            using (var stream = file.OpenWrite())
            {
                stream.Write(content, 0, content.Length);
            }
            Assert.That(file.ReadAllBytes(), Is.EquivalentTo(expected));
        }

        [Test]
        public void File_OpenWrite_Append()
        {
            var initial = new byte[] { 0x00, 0x01, 0x02 };
            var content = new byte[] { 0x03, 0x04 };
            var expected = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            var file = Provider.CreateFile("test.txt", initial);
            using (var stream = file.OpenWrite(WriteMode.Append))
            {
                stream.Write(content, 0, content.Length);
            }
            Assert.That(file.ReadAllBytes(), Is.EquivalentTo(expected));
        }

        [Test]
        public void File_OpenWrite_Truncate()
        {
            var initial = new byte[] { 0x00, 0x01, 0x02 };
            var content = new byte[] { 0x03, 0x04 };
            var file = Provider.CreateFile("test.txt", initial);
            using (var stream = file.OpenWrite(WriteMode.Truncate))
            {
                stream.Write(content, 0, content.Length);
            }
            Assert.That(file.ReadAllBytes(), Is.EquivalentTo(content));
        }

        [Test]
        public void File_Move_UsingIVirtualDirectory()
        {
            var file = Provider.CreateFile("test.txt", "test data");
            var dir = Provider.CreateDirectory("FolderMove");

            var newFile = file.MoveTo(dir);

            Assert.That(newFile, Is.Not.Null);
            Assert.That(newFile.Directory, Is.EqualTo(dir));
            Assert.That(Provider.GetFile("test.txt"), Is.Null);
        }

        [Test]
        public void File_Move_UsingIVirtualDirectoryAndName()
        {
            var file = Provider.CreateFile("test.txt", "test data");
            var dir = Provider.CreateDirectory("FolderMove");

            var newFile = file.MoveTo(dir, "newfile.txt");

            Assert.That(newFile, Is.Not.Null);
            Assert.That(newFile.Name, Is.EqualTo("newfile.txt"));
            Assert.That(newFile.Directory, Is.EqualTo(dir));
            Assert.That(Provider.GetFile("test.txt"), Is.Null);
        }

        [Test]
        public void File_Move_UsingDestinationVirtualPath()
        {
            var file = Provider.CreateFile("test.txt", "test data");
            var dir = Provider.CreateDirectory("FolderMove");

            var newFile = file.MoveTo("FolderMove");

            Assert.That(newFile, Is.Not.Null);
            Assert.That(newFile.Directory, Is.EqualTo(dir));
            Assert.That(Provider.GetFile("test.txt"), Is.Null);
        }

        [Test]
        public void File_Move_UsingDestinationVirtualPathAndName()
        {
            var file = Provider.CreateFile("test.txt", "test data");
            var dir = Provider.CreateDirectory("FolderMove");

            var newFile = file.MoveTo("FolderMove", "newfile.txt");

            Assert.That(newFile, Is.Not.Null);
            Assert.That(newFile.Name, Is.EqualTo("newfile.txt"));
            Assert.That(newFile.Directory, Is.EqualTo(dir));
            Assert.That(Provider.GetFile("test.txt"), Is.Null);
        }

        [Test]
        public void File_Move_CrossProvider()
        {
            using (var otherProvider = new InMemoryVirtualPathProvider())
            {
                var file = Provider.CreateFile("test.txt", "test data");
                var dir = otherProvider.CreateDirectory("FolderMove");

                var newFile = file.MoveTo(dir);

                Assert.That(newFile, Is.Not.Null);
                Assert.That(newFile.Directory, Is.EqualTo(dir));
                Assert.That(dir.GetFile("test.txt"), Is.EqualTo(newFile));
                Assert.That(Provider.GetFile("test.txt"), Is.Null);
            }
        }

        [Test]
        public void File_Copy_UsingIVirtualDirectory()
        {
            var file = Provider.CreateFile("test.txt", "test data");
            var dir = Provider.CreateDirectory("FolderCopy");

            var newFile = file.CopyTo(dir);

            Assert.That(newFile, Is.Not.Null);
            Assert.That(newFile.Directory, Is.EqualTo(dir));
            Assert.That(Provider.GetFile("test.txt"), Is.Not.Null);
        }

        [Test]
        public void File_Copy_UsingIVirtualDirectoryAndName()
        {
            var file = Provider.CreateFile("test.txt", "test data");
            var dir = Provider.CreateDirectory("FolderCopy");

            var newFile = file.CopyTo(dir, "newfile.txt");

            Assert.That(newFile, Is.Not.Null);
            Assert.That(newFile.Name, Is.EqualTo("newfile.txt"));
            Assert.That(newFile.Directory, Is.EqualTo(dir));
            Assert.That(Provider.GetFile("test.txt"), Is.Not.Null);
        }

        [Test]
        public void File_Copy_UsingDestinationVirtualPath()
        {
            var file = Provider.CreateFile("test.txt", "test data");
            var dir = Provider.CreateDirectory("FolderCopy");

            var newFile = file.CopyTo("FolderCopy");

            Assert.That(newFile, Is.Not.Null);
            Assert.That(newFile.Directory, Is.EqualTo(dir));
            Assert.That(Provider.GetFile("test.txt"), Is.Not.Null);
        }

        [Test]
        public void File_Copy_UsingDestinationVirtualPathAndName()
        {
            var file = Provider.CreateFile("test.txt", "test data");
            var dir = Provider.CreateDirectory("FolderCopy");

            var newFile = file.CopyTo("FolderCopy", "newfile.txt");

            Assert.That(newFile, Is.Not.Null);
            Assert.That(newFile.Name, Is.EqualTo("newfile.txt"));
            Assert.That(newFile.Directory, Is.EqualTo(dir));
            Assert.That(Provider.GetFile("test.txt"), Is.Not.Null);
        }

        [Test]
        public void File_Copy_CrossProvider()
        {
            using (var otherProvider = new InMemoryVirtualPathProvider())
            {
                var file = Provider.CreateFile("test.txt", "test data");
                var dir = otherProvider.CreateDirectory("FolderMove");

                var newFile = file.CopyTo(dir);

                Assert.That(newFile, Is.Not.Null);
                Assert.That(newFile.Directory, Is.EqualTo(dir));
                Assert.That(dir.GetFile("test.txt"), Is.EqualTo(newFile));
                Assert.That(Provider.GetFile("test.txt"), Is.Not.Null);
            }
        }
    }
}
