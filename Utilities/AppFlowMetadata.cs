using System;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Oauth2.v2;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;

namespace tower_admin_portal.Utilities
{
    // this current implementation doesnt work with asp.net core, only asp.net
    // need another approach
    public class AppFlowMetadata : FlowMetadata
    {
        private static readonly IAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "",
                    ClientSecret = ""
                },
                Scopes = new[] { Oauth2Service.Scope.UserinfoEmail },
                DataStore = new FileDataStore("Oauth2.Api.Auth.Store")
            });

        public override string GetUserId(Controller controller)
        {
            var user = controller.Session["user"];
            if (user == null)
            {
                user = Guid.NewGuid();
                controller.Session["user"] = user;
            }

            return user.ToString();
        }

        public override IAuthorizationCodeFlow Flow => flow;
    }
}