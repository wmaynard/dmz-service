using System;
using System.Linq;
using Dmz.Exceptions;
using Dmz.Extensions;
using Dmz.Interop;
using Dmz.Models;
using Dmz.Services;
using Dmz.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Models;
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
    private readonly BounceHandlerService _bounceHandler;
    private readonly AccountInitializationService _initService;
    private readonly SubscriptionService _subService;
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
        Require(Permissions.Player.View);

        return Forward("/player/v2/admin/details");
    }
    
    // Returns found information for a csv's accountIds
    [HttpGet, Route("lookup"), NoAuth]
    public ActionResult Lookup()
    {
        if (Token?.IsAdmin ?? false)
            Require(Permissions.Player.Search);

        return Forward("/player/v2/lookup", asAdmin: true);
    }

    /// <summary>
    /// This is used to log player into their player-service accounts, and return a corresponding token.
    /// Permissions are not necessary, since they are not site admins.
    /// </summary>
    // [HttpPost, Route("login"), NoAuth]
    // public ActionResult Login()
    // {
    //     return Forward("/player/v2/account/login");
    // }
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
#region Emails
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
    public ActionResult SendTwoFactorCode()
    {
        string email = Require<string>("email");
        string code = Require<string>("code");
        long expiration = Require<long>("expiration");
        
        PlayerServiceEmail.SendTwoFactorCode(email, code, expiration);
        return Ok();
    }
#endregion Emails

    [HttpGet, Route("account/unsubscribe"), NoAuth]
    public ActionResult Unsubscribe()
    {
        string success = _config.GetValuesFor(Audience.PlayerService).Require<string>("unsubscribeSuccessPage");
        string failure = _config.GetValuesFor(Audience.PlayerService).Require<string>("unsubscribeFailurePage");

        string email = null;
        
        try
        {
            email = Require<string>("email");

            _subService.Unsubscribe(email);
            return Redirect(success);
        }
        catch (Exception e)
        {
            Log.Error(Owner.Will, "Unable to unsubscribe a user from email.", data: new
            {
                Email = email
            }, exception: e);
            return Redirect(failure);
        }
    }
    
    [HttpGet, Route("account/resubscribe"), NoAuth]
    public ActionResult Resubscribe()
    {
        string success = _config.GetValuesFor(Audience.PlayerService).Require<string>("unsubscribeSuccessPage");
        string failure = _config.GetValuesFor(Audience.PlayerService).Require<string>("unsubscribeFailurePage");

        string email = null;
        
        try
        {
            email = Require<string>("email");

            _subService.Resubscribe(email);
            return Redirect(success);
        }
        catch (Exception e)
        {
            Log.Error(Owner.Will, "Unable to resubscribe a user from email.", data: new
            {
                Email = email
            }, exception: e);
            return Redirect(failure);
        }
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
    [Route("login")]
    public ActionResult Login()
    {
        ActionResult output = Forward("player/v2/account/login", out RumbleJson loginResponse);

        // Only accounts with SSO are allowed to hit the game server's initialization endpoint.
        RumbleJson player = loginResponse.Require<RumbleJson>("player");
        string encryptedToken = player.Require<string>("token");
        TokenInfo token = ApiService.Instance.ValidateToken(encryptedToken, "/dmz/account/login");
        if (!token.IsValidFor(Audience.DmzService))
            throw new InvalidTokenException(encryptedToken, token, "/dmz/account/login");
        
        bool hasSSO = (player.Optional<RumbleJson>("appleAccount")
            ?? player.Optional<RumbleJson>("googleAccount")
            ?? player.Optional<RumbleJson>("plariumAccount")) != null;
        bool hasConfirmedRumble = player
            .Optional<RumbleJson>("rumbleAccount")
            ?.Optional<int>("status") > 1; 

        if (!(hasSSO || hasConfirmedRumble))
            return output;

        _initService.InitializeIfNeeded(player, out string error);

        return error == null
            ? output
            : Problem(error);
    }

    [HttpGet, Route("account/salt"), NoAuth]
    public ActionResult GetRumbleSalt() => Forward("player/v2/account/salt", asAdmin: true);

    [HttpPatch, Route("account/recover"), NoAuth]
    public ActionResult RecoverAccount() => Forward("player/v2/account/recover");

    [HttpPatch, Route("account/reset"), NoAuth]
    public ActionResult Reset() => Forward("player/v2/account/reset");

    [HttpPatch, Route("account/password"), NoAuth]
    public ActionResult ChangePassword() => Forward("player/v2/account/password");
    
    [HttpGet, Route("token")]
    public ActionResult GetPlayerToken()
    {
        Require(Permissions.Player.GeneratePlayerTokens);
        
        string account = Require<string>(TokenInfo.FRIENDLY_KEY_ACCOUNT_ID);

        string screenname = null;
        int discriminator = -1;

        _apiService
            .Request("/player/v2/lookup")
            .AddAuthorization(Token)
            .AddParameter("accountIds", account)
            .OnSuccess(response =>
            {
                try
                {
                    RumbleJson[] results = response.Require<RumbleJson[]>("results");
                    RumbleJson player = results.First(result => result.Require<string>(TokenInfo.FRIENDLY_KEY_ACCOUNT_ID) == account);
                    screenname = player.Require<string>(TokenInfo.FRIENDLY_KEY_SCREENNAME);
                    discriminator = player.Require<int>(TokenInfo.FRIENDLY_KEY_DISCRIMINATOR);
                }
                catch (Exception e)
                {
                    Log.Error(Owner.Will, "Unable to lookup account for mock player token generation", data: new
                    {
                        AccountId = account
                    }, exception: e);
                }
            })
            .OnFailure(response => Log.Error(Owner.Will, "Request to player/lookup is invalid for mock token generation", data: new
            {
                Response = response
            }))
            .Get();

        if (screenname == null)
            throw new PlatformException("Mock player token generation failed", code: ErrorCode.MongoRecordNotFound);

        string token = _apiService
            .GenerateToken(
                accountId: account,
                screenname: screenname,
                email: $"{Token.Email} (as player)",
                discriminator: discriminator,
                audiences: Audience.All
            );

        return Ok(new RumbleJson
        {
            { "token", token }
        });
    }
    

    #endregion Rumble Account Login
    
    #region SSO Creation / Deletion

    [HttpPatch, Route("account/sso/apple"), NoAuth]
    public ActionResult AddApple() => FinalizeSSO("/player/v2/account/apple");

    [HttpDelete, Route("account/sso/appleAccount"), NoAuth]
    public ActionResult DeleteApple() => Forward("/player/v2/account/appleAccount");

    [HttpPatch, Route("account/sso/google"), NoAuth]
    public ActionResult AddGoogle() => FinalizeSSO("/player/v2/account/google");

    [HttpDelete, Route("account/sso/googleAccount"), NoAuth]
    public ActionResult DeleteGoogle() => Forward("/player/v2/account/googleAccount");

    [HttpPatch, Route("account/sso/plarium"), NoAuth]
    public ActionResult AddPlarium() => FinalizeSSO("/player/v2/account/plarium");
        
    [HttpDelete, Route("account/sso/plariumAccount"), NoAuth]
    public ActionResult DeletePlarium() => Forward("/player/v2/account/plariumAccount");

    [HttpPatch, Route("account/sso/rumble"), NoAuth]
    public ActionResult AddRumble()
    {
        try
        {
            return FinalizeSSO("/player/v2/account/rumble");
        }
        catch (ForwardingException e)
        {
            return Problem(e.Data);
        }
    }

    [HttpDelete, Route("account/sso/rumbleAccount"), NoAuth]
    public ActionResult DeleteRumble() => Forward("/player/v2/account/rumbleAccount");

    #endregion

    public ActionResult FinalizeSSO(string url)
    {
        ActionResult output = Forward(url, out RumbleJson response);
        string initError = null;

        RumbleJson player = response.Require<RumbleJson>("player");
        if (output is OkObjectResult)
            _initService.InitializeIfNeeded(player, out initError);
        
        return initError == null
            ? output
            : Problem(initError);
    }
}