using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Data;

namespace Dmz.Controllers;


[Route("dmz/market"), RequireAuth]
public class MarketController : DmzController
{
    [HttpGet, Route("player/read")]
    public ActionResult PlayerRead()
    {
        Forward("/player/v2/read", out RumbleJson response);

        // Token information is needed to accurately return decoded player information, such as email address (when relevant).
        response[TokenInfo.KEY_TOKEN_LEGACY_OUTPUT] = Token;
        response[TokenInfo.KEY_TOKEN_OUTPUT] = Token;
        
        return Ok(data: response);
    }
}