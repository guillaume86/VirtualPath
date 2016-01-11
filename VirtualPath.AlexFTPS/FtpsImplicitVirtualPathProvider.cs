using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AlexPilotti.FTPS.Client;
using AlexPilotti.FTPS.Common;
using VirtualPath.Common;

namespace VirtualPath.AlexFTPS
{
	[Export("ImplicitFTPS", typeof(IVirtualPathProvider))]
	public class FtpsImplicitVirtualPathProvider : FtpsVirtualPathProvider
    {
		public FtpsImplicitVirtualPathProvider(string host, int? port, string username, string password)
			: base (ESSLSupportMode.Implicit, host, port, username, password)
        {

        }

        public FtpsImplicitVirtualPathProvider(string host, string username, string password)
			: this(host, null, username, password)
        {

        }

		public FtpsImplicitVirtualPathProvider(string host, int port, string username, string password)
			: this(host, (int?)port, username, password)
        {

        }
    }
}
