using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Models.Permissions;
using TowerPortal.Models.Portal;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[AllowAnonymous]
[Route("portal/account")]
public class AccountController : PortalController
{
#pragma warning disable CS0649
    private readonly AccountService _accountService;
#pragma warning restore CS0649
    
    // Logs in through Google OAuth
    [Route("google-login")]
    public IActionResult GoogleLogin()
    {
        AuthenticationProperties properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleResponse")
        };

        return Challenge(properties, authenticationSchemes: GoogleDefaults.AuthenticationScheme);
    }
    
    // Logs out through Google OAuth
    [Route("google-logout")]
    public async Task<IActionResult> GoogleLogout()
    {
        await HttpContext.SignOutAsync();

        return Redirect("/portal/index");
    }

    // Google signin through Dev console
    [Route("signin-google")]
    public async Task<IActionResult> GoogleSignin()
    {
        return Ok();
    }

    // Restricts to company domain, then creates and sets default roles for a new user or logs in an existing user
    [Authorize(Policy = "CompanyStaffOnly")]
    [Route("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        
        Log.Info(Owner.Will, "SSO Sign in detected.");
        AuthenticateResult result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        Log.Info(Owner.Will, "SSO authenticated.");
        IEnumerable<Claim> claims = result?.Principal?.Identities?.FirstOrDefault()?.Claims;
        
        Account output = Account.FromGoogleClaims(claims);

        if (_accountService.GetByEmail(output.Email) == null)
        {
            Log.Info(owner: Owner.Nathan, message: "New portal account created.", data: new
            {
                PortalAccount = output
            });
            _accountService.Create(output);

            output.Permissions = Passport.GetDefaultPermissions(output.Email);
                
            _accountService.Update(output);
        }
        
        Log.Info(Owner.Will, "Account logged in successfully.", data: new
        {
            PortalAccount = output
        });

        return Redirect("/portal/index");
    }
}