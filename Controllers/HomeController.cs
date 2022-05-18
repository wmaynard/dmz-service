using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Authorize]
[Route("")]
public class HomeController : PlatformController
{
#pragma warning disable CS0649
    private readonly ApiService _apiService;
    private readonly DynamicConfigService _dynamicConfigService;
    private readonly AccountService _accountService;
#pragma warning restore CS0649

    [AllowAnonymous]
    [Route("")]
    [Route("index")]
    public IActionResult Index()
    {
        // Checking access permissions
        Account account = Account.FromGoogleClaims(User.Claims); // Models required for some reason?
        Account mongoAccount = _accountService.FindOne(mongo => mongo.Email == account.Email);
        if (mongoAccount == null)
        {
            return View();
        }

        mongoAccount.UpdateRolesToPermissions(); // Temporary to update existing accounts from roles to permissions
        
        // Hard coded admin upon login
        if (mongoAccount.Email == "nathan.mac@rumbleentertainment.com")
        {
            mongoAccount.Permissions.SetAdmin();
            _accountService.Update(mongoAccount);
        }
        
        ViewData["Permissions"] = mongoAccount.Permissions;
        Permissions currentPermissions = _accountService.CheckPermissions(mongoAccount);
        // Tab view permissions
        bool currentAdmin = currentPermissions.Admin;
        bool currentViewPlayer = currentPermissions.ViewPlayer;
        bool currentViewMailbox = currentPermissions.ViewMailbox;
        if (currentAdmin)
        {
            ViewData["CurrentAdmin"] = currentPermissions.Admin;
        }
        if (currentViewPlayer)
        {
            ViewData["CurrentViewPlayer"] = currentPermissions.ViewPlayer;
        }
        if (currentViewMailbox)
        {
            ViewData["CurrentViewMailbox"] = currentPermissions.ViewMailbox;
        }
        
        return View();
    }

    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});

    [AllowAnonymous]
    [Route("health")]
    [Route("portal/health")]
    public override ActionResult HealthCheck() => Ok(
        _apiService.HealthCheckResponseObject, 
        _dynamicConfigService.HealthCheckResponseObject
    );
}