using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Services;
using TowerPortal.Models;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Authorize]
[Route("portal")]
public class HomeController : PortalController
{
#pragma warning disable CS0649
    private readonly DynamicConfigService _dynamicConfigService;
    private readonly AccountService _accountService;
#pragma warning restore CS0649

    [AllowAnonymous]
    [Route("")]
    [Route("index")]
    public IActionResult Index()
    {
        // Checking access permissions
        Account account = Account.FromGoogleClaims(User.Claims);
        Account mongoAccount = _accountService.FindOne(mongo => mongo.Email == account.Email);
        if (mongoAccount == null)
        {
            return View();
        }

        mongoAccount.UpdateRolesToPermissions(); // Temporary to update existing accounts from roles to permissions
        
        // Hard coded admin upon login
        if (mongoAccount.Email == "nathan.mac@rumbleentertainment.com")
        {
            // mongoAccount.Permissions.SetAdmin();
            _accountService.Update(mongoAccount);
        }

        return View();
    }

    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
}