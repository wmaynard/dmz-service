using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;

namespace TowerPortal.Controllers;

[Route("/portal/leaderboard"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class LeaderboardController : PortalController
{
  #region Leaderboards
    // Gets all leaderboard ids
    [HttpGet, Route("list")]
    public ActionResult List()
    {
      Require(Permissions.Leaderboard.View_Page);

      return Forward("/leaderboard/admin/list");
    }
    
    // Gets a leaderboard's data by id
    [HttpGet, Route("")]
    public ActionResult Leaderboard()
    {
      Require(Permissions.Leaderboard.View_Page);

      return Forward("/leaderboard/archive");
    }
  #endregion
  
}