using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Data;

namespace Dmz.Controllers;

[ApiController, Route(template: "dmz")]
public class TopController : PlatformController
{   
    [HttpGet, Route("env"), RequireAuth]
    public ActionResult GetEnvironment() => Ok(new RumbleJson
    {
        { "gameSecret", PlatformEnvironment.GameSecret },
        { "rumbleSecret", PlatformEnvironment.RumbleSecret }
    });
}