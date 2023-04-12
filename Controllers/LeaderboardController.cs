using System.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Data;

// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("/dmz/leaderboard"), RequireAuth(AuthType.ADMIN_TOKEN)]
[EnableCors(PlatformStartup.CORS_SETTINGS_NAME)]
public class LeaderboardController : DmzController
{
    #pragma warning disable
    private readonly ConfigService _config;
    private readonly DynamicConfig _dynamicConfig;
    #pragma warning restore
    
    #region Leaderboards
    // Gets all leaderboard ids
    [HttpGet, Route("list")]
    public ActionResult List()
    {
        Require(Permissions.Leaderboard.View);

        return Forward("/leaderboard/admin/list");
    }
    
    // Gets a leaderboard's data by id
    [HttpGet, Route("archive")]
    public ActionResult FetchArchive()
    {
        Require(Permissions.Leaderboard.View);

        return Forward("/leaderboard/admin/archive");
    }

    [HttpGet]
    public ActionResult FetchLeaderboard()
    {
        Require(Permissions.Leaderboard.View);

        return Forward("leaderboard");
    }

    [HttpGet, Route("enrollments")]
    public ActionResult GetPlayerEnrollments()
    {
        Require(Permissions.Leaderboard.View);

        return Forward("leaderboard/admin/enrollments");
    }

    [HttpPatch, Route("season")]
    public ActionResult UpdateSeason()
    {
        Require(Permissions.Leaderboard.View);

        return Forward("leaderboard/admin/season");
    }

    [HttpPost, Route("mockScores"), IgnorePerformance]
    public ActionResult AddMockLeaderboardScores()
    {
        Require(Permissions.Leaderboard.View);

        return Forward("leaderboard/admin/mockScores");
    }

    [HttpPost, Route("rollover"), IgnorePerformance]
    public ActionResult ManualRollover()
    {
        Require(Permissions.Leaderboard.View);

        return Forward("leaderboard/admin/rollover");
    }

    [HttpGet, Route("shardStats")]
    public ActionResult GetShardStats()
    {
        Require(Permissions.Leaderboard.View);

        return Forward("leaderboard/admin/shardStats");
    }

    [HttpGet, Route("topShard"), NoAuth]
    public ActionResult GetTopShardScores()
    {
        const string CACHED_RESPONSE = "cachedLeaderboard";
        const string CACHE_UPDATED = "lastShardUpdate";
        
        long lastUpdated = _config.Value<long>(CACHE_UPDATED);

        RumbleJson config = _dynamicConfig.GetValuesFor(Audience.LeaderboardService);

        if (config == null)
            return BadRequest();
        
        // Fields supplied by DynamicConfig
        string type = config.Require<string>("discordLeaderboardId");
        long cacheLifetime = config.Require<long>("discordRefreshRate");
        int limit = config.Require<int>("discordReturnCount");

        if (lastUpdated > Timestamp.UnixTime - cacheLifetime)
            return Ok(_config.Value<RumbleJson>(CACHED_RESPONSE));

        bool success = true;
        RumbleJson leaderboard = null;
        RumbleJson[] playerInfo = null;
        _apiService
            .Request("/leaderboard/admin/topShard")
            .AddAuthorization(_dynamicConfig.AdminToken)
            .AddParameter("leaderboardId", type)
            .AddParameter("limit", limit.ToString())
            .OnSuccess(response => leaderboard = response.Require<string>("leaderboard"))
            .OnFailure(response =>
            {
                Log.Warn(Owner.Will, "Unable to fetch leaderboard shard.", data: response.AsRumbleJson);
                success = false;
            })
            .Get();

        if (!success || leaderboard == null)
            return BadRequest();

        string[] playerIds = leaderboard
            .Require<RumbleJson[]>("scores")
            .Select(json => json.Require<string>("accountId"))
            .ToArray();

        _apiService
            .Request("/player/v2/lookup")
            .AddAuthorization(_dynamicConfig.AdminToken)
            .AddParameter("accountIds", string.Join(',', playerIds))
            .OnSuccess(response => playerInfo = response.Require<RumbleJson[]>("results"))
            .OnFailure(response =>
            {
                Log.Warn(Owner.Will, "Unable to look up players from a leaderboard shard.", data: response.AsRumbleJson);
                success = false;
            })
            .Get();

        if (!success || playerInfo == null)
            return BadRequest();

        RumbleJson response = new RumbleJson
        {
            { "leaderboard", leaderboard },
            { "players", playerInfo }
        };
        
        _config.Update(CACHED_RESPONSE, response);
        _config.Update(CACHE_UPDATED, Timestamp.UnixTime);

        return Ok(response);
    }
    #endregion
}