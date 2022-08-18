using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TowerPortal.Models.Portal;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Authorize]
[Route("portal")]
public class HomeController : PortalController
{
    // Loads home page
    [AllowAnonymous]
    [Route("")]
    [Route("index")]
    public IActionResult Index()
    {
        return View();
    }

    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
}