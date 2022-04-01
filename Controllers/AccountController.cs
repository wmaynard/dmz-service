using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Web;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[AllowAnonymous]
[Route("account")]
public class AccountController : PlatformController
{
#pragma warning disable CS0649
    private readonly AccountService _accountService;
#pragma warning restore CS0649
    
    [Route("google-login")]
    public IActionResult GoogleLogin()
    {
        AuthenticationProperties properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleResponse")
        };

        return Challenge(properties, authenticationSchemes: GoogleDefaults.AuthenticationScheme);
    }

    [Authorize(Policy = "CompanyStaffOnly")]
    [Route("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        AuthenticateResult result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var claims = result?.Principal?.Identities?.FirstOrDefault()
            ?.Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

        return Json(claims);
    }
    
    public override ActionResult HealthCheck() => Ok(_accountService.HealthCheckResponseObject);
}