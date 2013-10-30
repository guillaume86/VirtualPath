using System;
using System.Collections.Generic;
using System.Linq;
using VirtualPath;

namespace VirtualPath.Common
{
    public static class VirtualPathExtension
    {
        public static Stack<string> TokenizeVirtualPath(this string str, IVirtualPathProvider pathProvider)
        {
            if (pathProvider == null)
                throw new ArgumentNullException("pathProvider");

            return TokenizeVirtualPath(str, pathProvider.VirtualPathSeparator);
        }

        public static Stack<string> TokenizeVirtualPath(this string str, string virtualPathSeparator)
        {
            if (string.IsNullOrEmpty(str))
                return new Stack<string>();

            var tokens = str.Split(new[] { virtualPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            return new Stack<string>(tokens.Reverse());
        }
    }
}
