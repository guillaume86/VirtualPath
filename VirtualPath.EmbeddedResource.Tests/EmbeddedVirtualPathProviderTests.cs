using System;
using System.Collections;
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
    public class EmbeddedVirtualPathProviderTests
    {
        [Test]
        public void ShouldEnumerateNodes()
        {
            //foreach (var name in GetResourceNames())
            //{
            //    Console.WriteLine(name);
            //}

            var provider = new EmbeddedResourceVirtualPathProvider(true, Assembly.GetExecutingAssembly());
            var root = provider.RootDirectory;
            foreach (var item in root)
            {
                Console.WriteLine("{0}: {1}", item.Name, item.IsDirectory);
            }

            var folder = root.GetDirectory("Folder1");
            Assert.That(folder, Is.Not.Null);

            var file = folder.GetFile("TextFile1.txt");
            Assert.That(file, Is.Not.Null);
            Console.WriteLine(file.ReadAllText());
        }

        public static IEnumerable<string> GetResourceNames()
        {
            var asm = Assembly.GetExecutingAssembly();
            var names = asm.GetManifestResourceNames();
            var resourceKey = ".resources";
            foreach (var resName in names)
            {
                if (resName.EndsWith(resourceKey))
                {
                    using (var stream = asm.GetManifestResourceStream(resName))
                    using (var reader = new System.Resources.ResourceReader(stream))
                    {
                        foreach (var entry in reader.Cast<DictionaryEntry>())
                        {
                            var name = (string)entry.Key;
                            yield return resName.Substring(0, resName.Length - resourceKey.Length) + "." + name + " = " + entry.Value.GetType();
                        }
                    }
                }
                else
                {
                    yield return resName;
                }
            }
        }
    }
}