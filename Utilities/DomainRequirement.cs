using Microsoft.AspNetCore.Authorization;

namespace tower_admin_portal.Utilities
{
    public class DomainRequirement : IAuthorizationRequirement
    {
        public string Domain { get; }

        public DomainRequirement(string domain)
        {
            Domain = domain;
        }
    }
}