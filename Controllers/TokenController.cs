using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;

namespace Dmz.Controllers;

[Route("dmz/token"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class TokenController : DmzController
{
    #region Player bans

    [HttpGet, Route("status")]
    public ActionResult BanStatus()
    {
        Require(Permissions.Token.View_Page);

        return Forward("/token/admin/status");
    }

    // Bans a player
    [HttpPatch, Route("ban")]
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