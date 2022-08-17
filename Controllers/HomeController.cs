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
#pragma warning disable CS0649
    private readonly AccountService _accountService;
#pragma warning restore CS0649

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