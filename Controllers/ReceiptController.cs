using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;

namespace Dmz.Controllers;

[Route("dmz/receipt"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class ReceiptController : DmzController
{
    #region Fetch receipts
    // Gets all receipts
    [HttpGet, Route("all")]
    public ActionResult All()
    {
        Require(Permissions.Receipt.View_Page);

        return Forward("/commerce/admin/all");
    }
    
    // Gets receipts for a player
    [HttpGet, Route("player")]
    public ActionResult Player(string accountId)
    {
        Require(Permissions.Receipt.View_Page);

        return Forward("/commerce/admin/player");
    }
    #endregion
}