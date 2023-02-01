using Microsoft.AspNetCore.Mvc;

namespace Dmz.Controllers;

[Route("dmz/game")]
public class GameServerController : DmzController
{
    [HttpPost, Route("offersForUser")]
    public ActionResult GetOffers()
    {
        return Forward("game/admin/offersForUser");
    }
}