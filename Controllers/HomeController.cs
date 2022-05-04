using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
        if (account.Email == null)
        {
            return View();
        }
        
        string admin = _accountService.CheckPermissions(account, "admin");
        string viewPlayer = _accountService.CheckPermissions(account, "viewPlayer");
        string viewMailbox = _accountService.CheckPermissions(account, "viewMailbox");
        // Tab view permissions
        ViewData["Admin"] = admin;
        ViewData["ViewPlayer"] = viewPlayer;
        ViewData["ViewMailbox"] = viewMailbox;
        
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