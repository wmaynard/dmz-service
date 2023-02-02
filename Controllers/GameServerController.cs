using Dmz.Utilities;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Utilities;

namespace Dmz.Controllers;

[Route("dmz/game")]
public class GameServerController : DmzController
{
    [HttpPost, Route("admin/offersForUser"), RequireAuth(AuthType.ADMIN_TOKEN)]
    public ActionResult AdminGetOffers()
    {
        // TODO: Enable this later
        // Require(Permissions.Player.ViewStoreOffers);
        
        // If DynamicConfig fails to return a value, this will hit the current environment URL.
        string gameServer = DynamicConfig.GetValuesFor(Audience.GameClient)?.Optional<string>("gameServerUrl") ?? "";

        return Forward(PlatformEnvironment.Url(gameServer, "/admin/offersForUser"));
    }
    
    [HttpGet, Route("offersForUser"), RequireAuth]
    public ActionResult GetOffers()
    {
        if (Token.IsAdmin)
            throw new PlatformException("Admin tokens are not valid on this endpoint.");

        // If DynamicConfig fails to return a value, this will hit the current environment URL.
        string gameServer = DynamicConfig.GetValuesFor(Audience.GameClient)?.Optional<string>("gameServerUrl") ?? "";

        return Forward(PlatformEnvironment.Url(gameServer, "/offersForUser"));
    }
}