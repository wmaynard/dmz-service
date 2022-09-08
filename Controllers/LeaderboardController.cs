using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;
// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("/dmz/leaderboard"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class LeaderboardController : DmzController
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