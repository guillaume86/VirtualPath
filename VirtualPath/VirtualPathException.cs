using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPath
{
    [Serializable]
    public class VirtualPathException : Exception
    {
        public VirtualPathException() { }
        public VirtualPathException(string message) : base(message) { }
        public VirtualPathException(string message, Exception inner) : base(message, inner) { }
        protected VirtualPathException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
