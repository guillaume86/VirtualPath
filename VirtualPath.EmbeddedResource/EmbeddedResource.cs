using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VirtualPath.EmbeddedResource
{
    public class EmbeddedResource
    {
        public EmbeddedResource(Assembly assembly, string resourcePath, string virtualPath)
        {
            this.Assembly = assembly;
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
            AssemblyLastModified = fileInfo.LastWriteTime;
            this.ResourcePath = resourcePath;
            this.VirtualPath = virtualPath;
        }

        public DateTime AssemblyLastModified { get; private set; }

        public string ResourcePath { get; private set; }

        public Stream GetStream()
        {
            return Assembly.GetManifestResourceStream(ResourcePath);
        }

        public Assembly Assembly { get; private set; }

        public string VirtualPath { get; private set; }
    }
}