using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;

namespace TowerPortal.Controllers;

[Route("portal/player"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class PlayerController : PortalController
{
    // Search for a player - screenname, id, etc are combined
    [HttpGet, Route("search")]
    public ActionResult Search()
    {
        Require(Permissions.Player.Search);

        return Forward("/player/v2/admin/search");
    }
    
    // Fetch player details
    [HttpGet, Route("details")]
    public ActionResult Details()
    {
        Require(Permissions.Player.View_Page);

        return Forward("/player/v2/admin/details");
    }

    // Sends a request to modify a player's screenname
    [HttpPost, Route("screenname")]
    public ActionResult Screenname()
    {
        Require(Permissions.Player.Screenname);

        return Forward("/player/v2/admin/screenname");
    }

    // Sends a request to add currency to a player's wallet
    // This requires the whole player component with field modified, as well as version += 1
    [HttpPost, Route("wallet/add")]
    public ActionResult WalletAdd()
    {
        Require(Permissions.Player.Add_Currency);

        return Forward("/player/v2/admin/component");
    }
    
    // Sends a request to remove currency from a player's wallet
    // This requires the whole player component with field modified, as well as version += 1
    [HttpPost, Route("wallet/remove")]
    public ActionResult WalletRemove()
    {
        Require(Permissions.Player.Remove_Currency);

        return Forward("/player/v2/admin/component");
    }
    
    
}