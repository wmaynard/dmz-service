using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Models;
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

    // Unlike all other endpoints, community needed raw text to come back for a Discord embed.
    // There's a limit of 4kb, so as little text as possible needs to be returned.
    [HttpGet, Route("topShard"), NoAuth]
    public ContentResult GetTopShardScores()
    {
        const string CACHED_RESPONSE = "cachedLeaderboard";
        const string CACHE_UPDATED = "lastShardUpdate";
            
        ContentResult badResult = new ContentResult
        {
            Content = "",
            ContentType = "text/plain",
            StatusCode = 400
        };
        
        try
        {
            _dynamicConfig.GetValuesFor(Audience.DmzService).Optional<string>("falafelophagus");

            long lastUpdated = _config.Value<long>(CACHE_UPDATED);

            RumbleJson config = _dynamicConfig.GetValuesFor(Audience.LeaderboardService);

            if (config == null)
                return badResult;
            
            // Fields supplied by DynamicConfig
            string type = config.Require<string>("discordLeaderboardId");
            long cacheLifetime = config.Require<long>("discordRefreshRate");
            int limit = config.Require<int>("discordReturnCount");

            if (lastUpdated > Timestamp.Now - cacheLifetime)
                return Content(_config.Value<string>(CACHED_RESPONSE));

            bool success = true;
            RumbleJson leaderboard = null;
            RumbleJson[] playerInfo = null;
            
            // Pull the top leaderboard shard out; in global leaderboards, currently, there's only one shard.
            // This will need to be revisited once global leaderboards are restructured.
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
                return badResult;

            string[] playerIds = leaderboard
                .Require<RumbleJson[]>("scores")
                .Select(json => json.Require<string>("accountId"))
                .ToArray();

            // Pull the player info for account level and screenname
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
                return null;
            
            // Combine the data into the requested CSV format
            string output = $"rank,score,level,name{Environment.NewLine}";
            foreach (RumbleJson scoreData in leaderboard.Require<RumbleJson[]>("scores"))
                try
                {
                    string id = scoreData.Require<string>(TokenInfo.FRIENDLY_KEY_ACCOUNT_ID);
                    int rank = scoreData.Require<int>("rank");
                    int score = scoreData.Require<int>("score");

                    RumbleJson account = playerInfo.First(json => json.Require<string>(TokenInfo.FRIENDLY_KEY_ACCOUNT_ID) == id);
                    int level = account.Require<int>("accountLevel");
                    string screenname = account.Require<string>("screenname");
                    output += $"{rank},{score},{level},{screenname}{Environment.NewLine}";
                }
                catch (Exception e)
                {
                    Log.Warn(Owner.Will, "Unable to parse leaderboard topShard data", exception: e);
                }

            output = output.Trim();
            
            _config.Update(CACHED_RESPONSE, output);
            _config.Update(CACHE_UPDATED, Timestamp.Now);

            return Content(output);
        }
        catch (Exception e)
        {
            Log.Warn(Owner.Will, "Unable to create Discord CSV from leaderboard data", exception: e);
            return badResult;
        }
        
    }
    #endregion
    
    #region Ladder

    [HttpGet, Route("/ladder/list")]
    public ActionResult ListLadderScores()
    {
        Require(Permissions.Leaderboard.View);

        return Forward("/leaderboard/admin/ladder/scores");
    }

    [HttpPatch, Route("/ladder/score")]
    public ActionResult UpdateLadderScore()
    {
        Require(Permissions.Leaderboard.UpdateLadderScores);

        return Forward("leaderboard/admin/ladder/score");
    }
    #endregion
}