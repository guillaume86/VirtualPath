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
    [Export("FTPS", typeof(IVirtualPathProvider))]
	[Export("ExplicitFTPS", typeof(IVirtualPathProvider))]
	public class FtpsVirtualPathProvider : FtpVirtualPathProvider
    {
		private ESSLSupportMode sslMode;

		public FtpsVirtualPathProvider(ESSLSupportMode sslMode, string host, int port, string username, string password)
			: base (new FTPSClient(), host, port, username, password)
        {
			this.sslMode = sslMode;
        }

        public FtpsVirtualPathProvider(string host, string username, string password)
			: this(ESSLSupportMode.DataChannelRequested, host, 21, username, password)
        {

        }

		public FtpsVirtualPathProvider(string host, int port, string username, string password)
			: this(ESSLSupportMode.DataChannelRequested, host, port, username, password)
        {

        }

        protected override void Connect()
        {
			Client.Connect(
				this.Host,
				this.Port ?? 21,
				new NetworkCredential(this.Username, this.Password),
				sslMode,
				null, null, 0, 0, 0, null);

            IsConnected = true;
        }
    }
}
