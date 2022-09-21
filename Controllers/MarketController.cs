using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;

namespace Dmz.Controllers;


[Route("dmz/market"), RequireAuth]
public class MarketController : DmzController
{
    [HttpGet, Route("player/read")]
    public ActionResult PlayerRead()
    {
        Forward("/player/v2/read", out GenericData response);

        // Token information is needed to accurately return decoded player information, such as email address (when relevant).
        response["tokenInfo"] = Token;
        
        return Ok(data: response);
    }
}