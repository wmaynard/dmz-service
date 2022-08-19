using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;

namespace TowerPortal.Controllers;

[Route("portal/player"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class PlayerController : PortalController
{
    #region Player lookup
    // Search for a player
    [HttpGet, Route("search")]
    public ActionResult Search()
    {
        Require(Permissions.Player.Search);

        return Forward("/player/v2/admin/search");
    }
    
    // Get player details
    [HttpGet, Route("details")]
    public ActionResult Details()
    {
        Require(Permissions.Player.View_Page);

        return Forward("/player/v2/admin/details");
    }
    #endregion

    #region Modifying data
    // Update a player's screenname
    [HttpPatch, Route("screenname")]
    public ActionResult Screenname()
    {
        Require(Permissions.Player.Screenname);

        return Forward("/player/v2/admin/screenname");
    }
    
    // Add currency to a player's wallet
    // Requires the whole player component with field modified, as well as version += 1
    [HttpPatch, Route("wallet/add")]
    public ActionResult WalletAdd()
    {
        Require(Permissions.Player.Add_Currency);

        return Forward("/player/v2/admin/component");
    }
    
    // Remove currency from a player's wallet
    // Requires the whole player component with field modified, as well as version += 1
    [HttpPatch, Route("wallet/remove")]
    public ActionResult WalletRemove()
    {
        Require(Permissions.Player.Remove_Currency);

        return Forward("/player/v2/admin/component");
    }
    #endregion
}