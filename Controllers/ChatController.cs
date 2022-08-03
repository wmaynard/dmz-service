using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Services;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Authorize]
[Route("/portal/chat")]
public class ChatController : PortalController
{
  private readonly DynamicConfigService _dynamicConfigService;
  private readonly AccountService       _accountService;

  [Route("announcements")]
  public async Task<IActionResult> Announcements()
  {
    return View();
  }

  [Route("player")]
  public async Task<IActionResult> Player()
  {
    return View();
  }
}