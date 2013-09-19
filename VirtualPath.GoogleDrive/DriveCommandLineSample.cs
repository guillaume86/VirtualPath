
using System;
using System.Diagnostics;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Util;
using Google.Apis.Services;
using System.Text;

namespace VirtualPath.GoogleDrive
{
    public class DriveCommandLineSample
    {
        public void Main(string CLIENT_ID, string CLIENT_SECRET)
        {
            // Register the authenticator and create the service
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description, CLIENT_ID, CLIENT_SECRET);
            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthorization);
            var service = new DriveService(new BaseClientService.Initializer()
            {
                Authenticator = auth
            });

            File body = new File();
            body.Title = "My document.txt";
            body.Description = "A test document";
            body.MimeType = "text/plain";

            byte[] byteArray = Encoding.UTF8.GetBytes("Hello world!!");
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

            FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, "text/plain");
            request.Upload();

            if (request.ResponseBody == null)
            {
                return;
            }
            var id = request.ResponseBody.Id;
            Console.WriteLine("File id: " + id);
            Console.WriteLine("Press Enter to end this process.");
            //Console.ReadLine();
        }

        private static IAuthorizationState GetAuthorization(NativeApplicationClient arg)
        {
            // Get the auth URL:
            IAuthorizationState state = new AuthorizationState(new[] { DriveService.Scopes.Drive.GetStringValue() });
            //state.Callback = new Uri(NativeApplicationClient.OutOfBandCallbackUrl);
            //Uri authUri = arg.RequestUserAuthorization(state);

            //// Request authorization from the user (by opening a browser window):
            //Process.Start(authUri.ToString());
            //Console.Write("  Authorization Code: ");
            ////string authCode = Console.ReadLine();
            //Console.WriteLine();
            string authCode = "4/wsziyROvEc6BmjV69hd_2QrW6OVJ.gjCETATDTQobOl05ti8ZT3a_0ceXgQI";

            // Retrieve the access token by using the authorization code:
            return arg.ProcessUserAuthorization(authCode, state);
        }
    }
}