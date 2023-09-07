using System;
using System.Linq;
using System.Threading.Tasks;
using Dmz.Interop;
using Dmz.Models.Portal;
using Dmz.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Data;

// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("/dmz/auth")]
public class AuthController : PlatformController
{
#pragma warning disable
    private readonly AccountService _accountService;
#pragma warning restore

    [HttpPost, Route("google"), NoAuth]
    public async Task<ActionResult> FromGoogle()
    {
        string token = Require<string>("token");

        SsoData data = await ValidateGoogleToken(token);
        
        if (string.IsNullOrWhiteSpace(data.Email))
            throw new PlatformException(message: "Email address not provided from Google token.", code: ErrorCode.Unauthorized);

        Account account = _accountService.GoogleLogin(data);

        bool whitelisted = false;

        // If we're in nonprod environments, try to whitelist the user as they log in for MongoDB network access.
        // Note that this does not grant them connection strings but merely lets them connect to our dev database
        // using other connection strings - e.g. for the game server in Unity.
        if (!PlatformEnvironment.IsProd)
            try
            {
                bool internalUser = PlatformService
                    .Require<DynamicConfig>()
                    .GetValuesFor(Audience.PlayerService)
                    .Require<string>("allowedSignupDomains")
                    .Split(',')
                    .Any(account.Email.Contains);

                if (!internalUser)
                    throw new PlatformException("User is not allowed to signup for internal tools.");
                
                Mongo.GetCurrentWhitelist(out MongoWhitelistEntry[] existing);
                
                if (existing.All(entry => entry.IpAddress != IpAddress))
                    whitelisted = Mongo.AddWhitelistEntry(account.Name, IpAddress);
                else
                    Log.Info(Owner.Will, "Whitelist entry already exists for user", data: new
                    {
                        PortalAccount = account,
                        IpAddress = IpAddress
                    });
            }
            catch (Exception e)
            {
                Log.Error(Owner.Will, "Something went wrong checking or updating the Mongo IP whitelist", exception: e);
            }

        string platformToken = GenerateToken(account);

        return Ok(new RumbleJson
        {
            { "account", account },
            { "platformToken", platformToken },
            { "permissions", account.Permissions },
            { "gameSecret", PlatformEnvironment.GameSecret },
            { "rumbleSecret", PlatformEnvironment.RumbleSecret },
            { "gitlabPat", PlatformEnvironment.Optional<string>("GITLAB_PAT") ?? "not found" },
            { "whitelistUpdated", whitelisted }
        });
    }

    /// <summary>
    /// Validates the provided Google token.
    /// </summary>
    /// <returns>An SsoData object representing the Google JWT claims.</returns>
    /// <exception cref="PlatformException"></exception>
    private async Task<SsoData> ValidateGoogleToken(string token)
    {
        try
        {
            return await GoogleJsonWebSignature.ValidateAsync(token);
        }
        catch (Exception e)
        {
            throw new PlatformException("Google token failed validation", inner: e);
        }
    }

    /// <summary>
    /// Returns a platform admin JWT to be used for all future requests.
    /// </summary>
    /// <param name="account">The portal account to use for token generation.</param>
    /// <returns>A Platform JWT</returns>
    private string GenerateToken(Account account)
    {
        _apiService
            .Request("/secured/token/generate")
            .SetPayload(new RumbleJson
            {
                { "aid", account.Id },
                { "accountId", account.Id },
                { "screenname", account.Name },
                { "origin", PlatformEnvironment.ServiceName },
                { "days", PlatformEnvironment.IsProd ? 1 : 4 },
                { "email", account.Email },
                { "key", PlatformEnvironment.RumbleSecret }
            })
            .OnFailure(response =>
            {
                Log.Error(Owner.Will, "Unable to generate token.", data: new
                {
                    accountId = account.Id,
                    Response = response.AsRumbleJson
                });
            })
            .Post(out RumbleJson json, out int code);

        if (!code.Between(200, 299))
            throw new PlatformException("DMZ token generation failed.");

        return json.Require<RumbleJson>("authorization").Require<string>("token");
    }
}