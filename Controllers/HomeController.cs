using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;
using TowerPortal.Services;

namespace TowerPortal.Controllers
{
    [Authorize]
    [Route("portal")]
    public class HomeController : PlatformController
    {
#pragma warning disable CS0169
	    private readonly ApiService _apiService;
	    private readonly DynamicConfigService _dynamicConfigService;
#pragma warning restore CS0169
        
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
        [Route("health")]
        public override ActionResult HealthCheck() => Ok(
	        _apiService.HealthCheckResponseObject, 
	        _dynamicConfigService.HealthCheckResponseObject
	    );
    }
}