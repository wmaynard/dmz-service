using Microsoft.AspNetCore.Mvc;

namespace Dmz.Controllers;

[Route("game")]
public class GameServerController : DmzController
{
    [HttpPost, Route("offersForUser")]
    public ActionResult GetOffers()
    {
        return Forward("game/admin/offersForUser");
    }
}