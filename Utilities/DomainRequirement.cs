using Microsoft.AspNetCore.Authorization;

namespace TowerPortal.Utilities
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