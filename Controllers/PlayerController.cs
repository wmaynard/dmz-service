using Dmz.Extensions;
using Dmz.Services;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;
// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("dmz/player"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class PlayerController : DmzController
{
    #region Player lookup
    // Search for a player
    [HttpGet, Route("search")]
    public ActionResult Search()
    {
        Require(Permissions.Player.Search); // necessary? maybe change to view_page

        return Forward("/player/v2/admin/search");
    }
    
    // Get player details
    [HttpGet, Route("details")]
    public ActionResult Details()
    {
        Require(Permissions.Player.View_Page);

        return Forward("/player/v2/admin/details");
    }
    
    // Returns found information for a csv's accountIds
    [HttpGet, Route("lookup")]
    public ActionResult Lookup()
    {
        Require(Permissions.Player.Search);

        return Forward("/player/v2/lookup");
    }

    /// <summary>
    /// This is used to log player into their player-service accounts, and return a corresponding token.
    /// Permissions are not necessary, since they are not site admins.
    /// </summary>
    [HttpPost, Route("login"), NoAuth]
    public ActionResult Login()
    {
        return Forward("/player/v2/login");
    }
    #endregion

    #region Modifying data
    // Update a player's screenname
    [HttpPatch, Route("screenname")]
    public ActionResult Screenname()
    {
        Require(Permissions.Player.Screenname);

        string aid = Require<string>(key: "accountId");
        _apiService.ForceRefresh(aid);

        return Forward("/player/v2/admin/screenname");
    }
    
    // Unlink account
    [HttpDelete, Route("unlink")]
    public ActionResult Unlink()
    {
        Require(Permissions.Player.Unlink_Accounts);

        return Forward("/player/v2/admin/profiles/unlink");
    }
    
    // Requires the whole player component with field modified, as well as version += 1
    [HttpPatch, Route("update")]
    public ActionResult Update()
    {
        Require(Permissions.Player.Update);

        return Forward("/player/v2/admin/component");
    }
    
    // Temporary wallet update before player service is updated
    [HttpPatch, Route("update/wallet")]
    public ActionResult UpdateWallet()
    {
        Require(Permissions.Player.Update);
        
        string aid = Require<string>(key: "accountId");
        _apiService.ForceRefresh(aid);

        return Forward("/player/v2/admin/currency");
    }
    #endregion
}