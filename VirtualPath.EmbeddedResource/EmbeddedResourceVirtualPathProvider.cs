using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using VirtualPath.Common;

namespace VirtualPath.EmbeddedResource
{
    public class EmbeddedResourceVirtualPathProvider : AbstractVirtualPathProviderBase
    {
        private readonly bool ExcludeAssemblyNameFromPath;
        private readonly Dictionary<string, EmbeddedResource> Resources;
        public EmbeddedResourceVirtualPathProvider(params Assembly[] assemblies)
            : this(false, assemblies)
        {

        }

        public EmbeddedResourceVirtualPathProvider(bool excludeAssemblyNameFromPath, params Assembly[] assemblies)
        {
            this.ExcludeAssemblyNameFromPath = excludeAssemblyNameFromPath;
            this.Resources = new Dictionary<string, EmbeddedResource>();
            Array.ForEach(assemblies, a => Add(a));
        }

        public void Add(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            foreach (var resourcePath in assembly.GetManifestResourceNames())
            {
                var virtualPath = resourcePath;
                if (ExcludeAssemblyNameFromPath && resourcePath.StartsWith(assemblyName))
                {
                    virtualPath = resourcePath.Substring(assemblyName.Length).TrimStart('.');
                }

                var tokens = virtualPath.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length > 1) // Handle file extension
                {
                    var extension = tokens.Last();
                    tokens = tokens.Take(tokens.Length - 1).ToArray();
                    tokens[tokens.Length - 1] += "." + extension;
                }
                virtualPath = this.VirtualPathSeparator + tokens.Aggregate((a, b) => CombineVirtualPath(a, b));

                if (Resources.ContainsKey(virtualPath))
                    throw new ArgumentException(String.Format("Some resources use the same key: {0}", resourcePath));
                Resources[virtualPath] = new EmbeddedResource(assembly, resourcePath, virtualPath);
            }
        }

        internal IEnumerable<EmbeddedResource> GetResources(string virtualPath)
        {
            return Resources
                .Where(r => r.Key.StartsWith(virtualPath)) // Filter directory
                .Select(r => r.Value);
        }

        internal IEnumerable<string> GetDirectories(IEnumerable<EmbeddedResource> resources)
        {
            return resources.Select(r => r.ResourcePath.Split('.'))
                .Select(tokens => tokens.Take(tokens.Length - 2)) // Remove filename
                .Where(tokens => tokens.Count() > 0)
                .Select(tokens => String.Join(this.VirtualPathSeparator, tokens))
                .Distinct();
        }

        internal EmbeddedResourceVirtualFile GetEmbeddedFile(string virtualPath)
        {
            var tokens = new Stack<string>(virtualPath.Split(this.VirtualPathSeparator[0]));
            var filename = tokens.Pop();
            var directory = GetEmbeddedDirectory(tokens);
            var resource = GetResourceFromVirtualPath(virtualPath);
            if (resource != null)
                return new EmbeddedResourceVirtualFile(this, directory, resource);
            return null;
        }

        private EmbeddedResourceVirtualDirectory GetEmbeddedDirectory(Stack<string> tokens)
        {
            tokens = new Stack<string>(tokens);
            if(!tokens.Any())
            {
                return (EmbeddedResourceVirtualDirectory)RootDirectory;
            }
            var name = tokens.Pop();
            return new EmbeddedResourceVirtualDirectory(this, GetEmbeddedDirectory(tokens), name);
        }

        internal EmbeddedResource GetResourceFromVirtualPath(string virtualPath)
        {
            var key = virtualPath;
            if (Resources.ContainsKey(key))
            {
                return Resources[key];
            }
            return null;
        }

        public override IVirtualDirectory RootDirectory
        {
            get { return new EmbeddedResourceVirtualDirectory(this, null, null); }
        }

        public override string VirtualPathSeparator
        {
            get { return "/"; }
        }

        public override string RealPathSeparator
        {
            get { return "/"; }
        }
    }
}