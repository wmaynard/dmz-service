using Dmz.Utilities;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Extensions;
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

    /// <summary>
    /// This is used to log player into their player-service accounts, and return a corresponding token.
    /// Permissions are not necessary, since they are not site admins.
    /// </summary>
    [HttpPost, Route("login"), NoAuth]
    public ActionResult Login() => Forward("/player/v2/login");
    #endregion

    #region Modifying data
    // Update a player's screenname
    [HttpPatch, Route("screenname")]
    public ActionResult Screenname()
    {
        Require(Permissions.Player.Screenname);

        string aid = Require<string>(key: "accountId");
        _apiService
            .Request(PlatformEnvironment.Url("/token/admin/invalidate"))
            .AddAuthorization(ContextHelper.Token.Authorization)
            .SetPayload(new GenericData
                        {
                            {"aid", aid}
                        })
            .OnSuccess((sender, response) =>
                       {
                           Log.Info(owner: Owner.Nathan,
                                    message: "Invalidating token to force user refresh due to a portal request.");
                       })
            .OnFailure((sender, response) =>
                       {
                           Log.Error(owner: Owner.Nathan,
                                     message:
                                     "Failed to invalidate token when attempting to force user refresh due to a portal request.");
                       })
            .Patch();

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
        _apiService
            .Request(PlatformEnvironment.Url("/token/admin/invalidate"))
            .AddAuthorization(ContextHelper.Token.Authorization)
            .SetPayload(new GenericData
                        {
                            {"aid", aid}
                        })
            .OnSuccess((sender, response) =>
                       {
                           Log.Info(owner: Owner.Nathan,
                                    message: "Invalidating token to force user refresh due to a portal request.");
                       })
            .OnFailure((sender, response) =>
                       {
                           Log.Error(owner: Owner.Nathan,
                                     message:
                                     "Failed to invalidate token when attempting to force user refresh due to a portal request.");
                       })
            .Patch();

        return Forward("/player/v2/admin/currency");
    }
    #endregion
}