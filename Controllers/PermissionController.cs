using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Web;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Authorize]
[Route("permission")]
public class PermissionController : PlatformController
{
    private readonly ApiService _apiService;
    private readonly DynamicConfigService _dynamicConfigService;
    private readonly AccountService _accountService;

    [Route("search")]
    public async Task<IActionResult> Search(string query)
    {
        ViewData["Message"] = "User search";
        bool allowed = _accountService.Test();
        ViewData["Admin"] = true;

        if (allowed == null)
        {
            return View("Error");
        }

        if (query != null)
        {
            
        }

        return View();
    }

    [Route("account")]
    public async Task<IActionResult> Account(string account)
    {
        ViewData["Account"] = account;

        return View();
    }
    
    [Route("health")]
    public override ActionResult HealthCheck() => Ok(_apiService.HealthCheckResponseObject, _dynamicConfigService.HealthCheckResponseObject);
}