using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;

namespace TowerPortal.Controllers;

[Authorize]
[Route("")]
public class HomeController : PlatformController
{
#pragma warning disable CS0649
    private readonly ApiService _apiService;
    private readonly DynamicConfigService _dynamicConfigService;
#pragma warning restore CS0649
    
    [AllowAnonymous]
    [Route("")]
    [Route("index")]
    public IActionResult Index() => View();

    [Route("privacy")]
    public IActionResult Privacy() => View();

    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});

    [AllowAnonymous]
    public override ActionResult HealthCheck() => Ok(
        _apiService.HealthCheckResponseObject, 
        _dynamicConfigService.HealthCheckResponseObject
    );
}