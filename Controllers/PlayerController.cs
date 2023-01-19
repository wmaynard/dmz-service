using System;
using Dmz.Extensions;
using Dmz.Interop;
using Dmz.Models;
using Dmz.Services;
using Dmz.Utilities;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("dmz/player"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class PlayerController : DmzController
{
#pragma warning disable
    private readonly DynamicConfig _config;
    private readonly ScheduledEmailService _emailService;
#pragma warning restore
    
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
        return Forward("/player/v2/account/login");
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

        Body["origin"] = PlatformEnvironment.Url("dmz/player/update");

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

    [HttpPatch, Route("account/link")]
    public ActionResult LinkAccounts()
    {
        Require(Permissions.Player.Update);

        bool force = Optional<bool>("force");

        if (force)
            Require(Permissions.Player.ForceAccountLink);

        return Forward("/player/v2/admin/accountLink");
    }
    #endregion
    
#region Rumble Account Login
    [HttpPost, Route("account/confirmation")]
    public ActionResult SendConfirmationEmail()
    {
        string email = Require<string>("email");
        string accountId = Require<string>("accountId");
        string code = Require<string>("code");
        long expiration = Require<long>("expiration");
        
        PlayerServiceEmail.SendConfirmation(email, accountId, code, expiration);

        return Ok();
    }

    [HttpPost, Route("account/welcome")]
    public ActionResult SendWelcomeEmail()
    {
        string email = Require<string>("email");

        _emailService.Create(PlayerServiceEmail.ScheduleWelcome(email));

        return Ok();
    }

    [HttpPost, Route("account/reset")]
    public ActionResult SendPasswordResetEmail()
    {
        string email = Require<string>("email");
        string accountId = Require<string>("accountId");
        string code = Require<string>("code");
        long expiration = Require<long>("expiration");
        
        PlayerServiceEmail.SendPasswordReset(email, accountId, code, expiration);

        return Ok();
    }

    [HttpPost, Route("account/notification")]
    public ActionResult SendLoginNotification()
    {
        string email = Require<string>("email");
        string device = Require<string>("device");
        
        PlayerServiceEmail.SendNewDeviceLogin(email, device);

        return Ok();
    }

    [HttpPost, Route("account/2fa")]
    public ActionResult SendTwoFactorCoe()
    {
        string email = Require<string>("email");
        string code = Require<string>("code");
        long expiration = Require<long>("expiration");
        
        PlayerServiceEmail.SendTwoFactorCode(email, code, expiration);
        return Ok();
    }

    /// <summary>
    /// Sends the player email click data to player-service.
    /// The response from player-service includes a redirect URL to send users from DMZ -> public website.
    /// </summary>
    [HttpGet, Route("account/confirm"), NoAuth]
    public ActionResult AcceptConfirmation()
    {
        Forward("/player/v2/account/confirm", out RumbleJson response);

        try
        {
            string url = response.Require<RumbleJson>("loginRedirect").Require<string>("url");
            return Redirect(url);
        }
        catch (Exception e)
        {
            Log.Error(Owner.Will, "Unable to redirect user by instruction from player-service.", data: new
            {
                Response = response
            }, exception: e);
        }

        try
        {
            string url = _config
                .GetValuesFor(Audience.PlayerService)
                .Require<string>("confirmationFailurePage")
                .Replace("{reason}", "noResponse");
            return Redirect(url);
        }
        catch (Exception e)
        {
            Log.Error(Owner.Will, "Unable to redirect user from dynamic config as backup.", exception: e);
        }
        
        return Problem();
    }

    [HttpPost, Route("account/login"), NoAuth]
    public ActionResult LoginWithGoogle() => Forward("player/v2/account/login");

    [HttpGet, Route("account/salt"), NoAuth]
    public ActionResult GetRumbleSalt() => Forward("player/v2/account/salt", asAdmin: true);

    [HttpPatch, Route("account/recover"), NoAuth]
    public ActionResult RecoverAccount() => Forward("player/v2/account/recover");

    [HttpPatch, Route("account/reset"), NoAuth]
    public ActionResult Reset() => Forward("player/v2/account/reset");

    [HttpPatch, Route("account/password"), NoAuth]
    public ActionResult ChangePassword() => Forward("player/v2/account/password");
    

    #endregion Rumble Account Login
}