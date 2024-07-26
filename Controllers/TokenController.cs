using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Utilities.JsonTools;

// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("dmz/token"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class TokenController : DmzController
{
    #region Player bans

    [HttpGet, Route("status")]
    public ActionResult BanStatus()
    {
        Require(Permissions.Token.View);

        return Forward("/token/admin/status");
    }

    // Bans a player
    [HttpPost, Route("ban")]
    public ActionResult Ban()
    {
        Require(Permissions.Token.Ban);

        return Forward("/token/admin/ban");
    }

    // Unbans a player
    [HttpPatch, Route("unban")]
    public ActionResult Unban()
    {
        Require(Permissions.Token.Unban);

        return Forward("/token/admin/unban");
    }

    [HttpGet, Route("audienceList")]
    public ActionResult GetAudiences()
    {
        Require(Permissions.Token.View);
        
        RumbleJson output = RumbleJson.FromDictionary(Enum
            .GetValues<Audience>()
            .ToDictionary(
                keySelector: audience => audience.GetDisplayName(),
                elementSelector: audience => (int)audience
        ));
        return Ok(output);
    }
    #endregion

    #region Token invalidation
    // Invalidate a player's token
    [HttpPatch, Route("invalidate")]
    public ActionResult Invalidate()
    {
        Require(Permissions.Token.Invalidate);

        return Forward("/token/admin/invalidate");
    }
    #endregion
}