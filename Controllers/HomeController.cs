using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;

namespace TowerPortal.Controllers
{
    [Authorize]
    public class HomeController : PlatformController
    {
        [AllowAnonymous]
        public IActionResult Index() => View();

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});

        [AllowAnonymous]
        public IActionResult health() => Ok();
        
        public override ActionResult HealthCheck() => Ok();
    }
}