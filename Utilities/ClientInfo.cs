using Rumble.Platform.Common.Utilities;

namespace tower_admin_portal.Utilities
{
    public class ClientInfo
    {
        public static ClientInfo Load()
        {
            string projectId = "tower-admin-portal";
            string clientId = PlatformEnvironment.Variable(name: "GOOGLE_CLIENT_ID");
            string clientSecret = PlatformEnvironment.Variable(name: "GOOGLE_CLIENT_SECRET");
            return new ClientInfo(projectId, clientId, clientSecret);
        }

        private ClientInfo()
        {
            Load();
        }

        private ClientInfo(string projectId, string clientId, string clientSecret)
        {
            ProjectId = projectId;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }
        
        public string ProjectId { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }
    }
}