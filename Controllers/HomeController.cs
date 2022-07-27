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
            mongoAccount.Permissions.SetAdmin();
            _accountService.Update(mongoAccount);
        }
        
        mongoAccount.Permissions.SetUser();
        _accountService.Update(mongoAccount);
        
        ViewData["Permissions"] = mongoAccount.Permissions;
        Permissions currentPermissions = _accountService.CheckPermissions(mongoAccount);
        // Tab view permissions
        bool currentAdmin = currentPermissions.Admin;
        bool currentManagePermissions = currentPermissions.ManagePermissions;
        bool currentViewPlayer = currentPermissions.ViewPlayer;
        bool currentViewMailbox = currentPermissions.ViewMailbox;
        bool currentViewToken = currentPermissions.ViewToken;
        bool currentViewConfig = currentPermissions.ViewConfig;
        if (currentAdmin)
        {
            ViewData["CurrentAdmin"] = currentPermissions.Admin;
        }
        if (currentManagePermissions)
        {
            ViewData["CurrentManagePermissions"] = currentPermissions.ManagePermissions;
        }
        if (currentViewPlayer)
        {
            ViewData["CurrentViewPlayer"] = currentPermissions.ViewPlayer;
        }
        if (currentViewMailbox)
        {
            ViewData["CurrentViewMailbox"] = currentPermissions.ViewMailbox;
        }
        if (currentViewToken)
        {
            ViewData["CurrentViewToken"] = currentPermissions.ViewToken;
        }
        if (currentViewConfig)
        {
            ViewData["CurrentViewConfig"] = currentPermissions.ViewConfig;
        }
        
        return View();
    }

    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
}