using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace tower_admin_portal.Utilities
{
    public class DomainHandler : AuthorizationHandler<DomainRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DomainRequirement requirement)
        {
            Claim emailClaim = context.User.FindFirst(claim => claim.Type == ClaimTypes.Email);
            if (emailClaim != null && emailClaim.Value.EndsWith($@"{requirement.Domain}"))
            {
                context.Succeed(requirement);
            }
            
            return Task.CompletedTask;
        }
    }
}