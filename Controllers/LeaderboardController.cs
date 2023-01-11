using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;

// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("/dmz/leaderboard"), RequireAuth(AuthType.ADMIN_TOKEN)]
[EnableCors(PlatformStartup.CORS_SETTINGS_NAME)]
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
    [HttpGet, Route("archive")]
    public ActionResult FetchArchive()
    {
        Require(Permissions.Leaderboard.View_Page);

        return Forward("/leaderboard/admin/archive");
    }

    [HttpGet]
    public ActionResult FetchLeaderboard()
    {
        Require(Permissions.Leaderboard.View_Page);

        return Forward("leaderboard");
    }

    [HttpGet, Route("enrollments")]
    public ActionResult GetPlayerEnrollments()
    {
        Require(Permissions.Leaderboard.View_Page);

        return Forward("leaderboard/admin/enrollments");
    }

    [HttpPatch, Route("season")]
    public ActionResult UpdateSeason()
    {
        Require(Permissions.Leaderboard.View_Page);

        return Forward("leaderboard/admin/season");
    }

    [HttpPost, Route("mockScores"), IgnorePerformance]
    public ActionResult AddMockLeaderboardScores()
    {
        Require(Permissions.Leaderboard.View_Page);

        return Forward("leaderboard/admin/mockScores");
    }

    [HttpPost, Route("rollover"), IgnorePerformance]
    public ActionResult ManualRollover()
    {
        Require(Permissions.Leaderboard.View_Page);

        return Forward("leaderboard/admin/rollover");
    }

    [HttpGet, Route("shardStats")]
    public ActionResult GetShardStats()
    {
        Require(Permissions.Leaderboard.View_Page);

        return Forward("leaderboard/admin/shardStats");
    }
    #endregion
}