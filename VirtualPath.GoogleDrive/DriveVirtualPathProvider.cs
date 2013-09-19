using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Drive.v2;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Util;
using VirtualPath.Common;

namespace VirtualPath.GoogleDrive
{
    [Export("GoogleDrive", typeof(IVirtualPathProvider))]
    public class DriveVirtualPathProvider : AbstractVirtualPathProviderBase
    {
        public DriveService Client { get; private set; }

        public DriveVirtualPathProvider(DriveService client)
        {
            Client = client;

            var files = client.Files.List();
            foreach (var file in files.Execute().Items)
            {
                Console.WriteLine(file.OriginalFilename ?? file.Title);
            }
        }

        public DriveVirtualPathProvider(string appName, string clientId, string clientSecret, string authToken)
            : this(BuildService(appName, clientId, clientSecret, authToken))
        {

        }

        public override IVirtualDirectory RootDirectory
        {
            get { throw new NotImplementedException(); }
        }

        public override string VirtualPathSeparator
        {
            get { return "/"; }
        }

        public override string RealPathSeparator
        {
            get { return "/"; }
        }

        internal static DriveService BuildService(string appName, string clientId, string clientSecret, string authToken)
        {
            // Register the authenticator and create the service
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description, clientId, clientSecret);
            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthorizationDelegate(authToken));
            var service = new DriveService(new BaseClientService.Initializer()
            {
                Authenticator = auth
            });
            return service;

            //var initilizer = new BaseClientService.Initializer();
            //initilizer.ApplicationName = appName;
            //initilizer.Authenticator = CreateAuthenticator(clientId, clientSecret);
            //return new DriveService(initilizer);
        }

        private static Func<NativeApplicationClient, IAuthorizationState> GetAuthorizationDelegate(string authToken)
        {
            return (NativeApplicationClient arg) =>
            {
                // Get the auth URL:
                IAuthorizationState state = new AuthorizationState(new[] { DriveService.Scopes.Drive.GetStringValue() });
                state.Callback = new Uri(NativeApplicationClient.OutOfBandCallbackUrl);
                //Uri authUri = arg.RequestUserAuthorization(state);

                //// Request authorization from the user (by opening a browser window):
                //Process.Start(authUri.ToString());
                //Console.Write("  Authorization Code: ");
                ////string authCode = Console.ReadLine();
                //Console.WriteLine();

                // Retrieve the access token by using the authorization code:
                return arg.ProcessUserAuthorization(authToken, state);
            };
        }

        //private static IAuthenticator CreateAuthenticator(string clientId, string clientSecret)
        //{
        //    var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
        //    {
        //        ClientIdentifier = clientId,
        //        ClientSecret = clientSecret
        //    };
        //    return new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthorization);
        //}

        //private static IAuthorizationState GetAuthorization(NativeApplicationClient client)
        //{
        //    // You should use a more secure way of storing the key here as
        //    // .NET applications can be disassembled using a reflection tool.
        //    const string STORAGE = "google.samples.dotnet.tasks";
        //    const string KEY = "y},drdzf11x9;87";
        //    //string scope = DriveService.Scopes.DriveAppdata.ToString(); // .GetStringValue();

        //    // Check if there is a cached refresh token available.
        //    IAuthorizationState state = null;
        //    state = AuthorizationMgr.GetCachedRefreshToken(STORAGE, KEY);
        //    if (state != null)
        //    {
        //        try
        //        {
        //            client.RefreshToken(state);
        //            return state; // Yes - we are done.
        //        }
        //        catch (DotNetOpenAuth.Messaging.ProtocolException ex)
        //        {
        //            // WriteError("Using existing refresh token failed: " + ex.Message);
        //        }
        //    }

        //    // Retrieve the authorization from the user.
        //    state = AuthorizationMgr.RequestNativeAuthorization(client, SCOPES);
        //    AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state);
        //    return state;
        //}

        //public static readonly string[] SCOPES = new String[]
        //{
        //    "https://www.googleapis.com/auth/drive.file",
        //    "https://www.googleapis.com/auth/userinfo.email",
        //    "https://www.googleapis.com/auth/userinfo.profile",
        //    "https://www.googleapis.com/auth/drive.install"
        //};
    }
}
