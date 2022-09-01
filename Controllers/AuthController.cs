using System;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;
using TowerPortal.Models.Portal;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Route("/portal/auth")]
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
        {
            throw new PlatformException(message: "Email address not provided from Google token.", code: ErrorCode.Unauthorized);
        }

        Account account = _accountService.GoogleLogin(data);
        string platformToken = GenerateToken(account);

        return Ok(new GenericData
        {
            { "platformToken", platformToken },
            { "permissions", account.Permissions }
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
            .SetPayload(new GenericData
            {
                { "aid", account.Id },
                { "screenname", account.Name },
                { "origin", PlatformEnvironment.ServiceName },
                { "days", PlatformEnvironment.IsProd ? 1 : 4 },
                { "email", account.Email },
                { "key", PlatformEnvironment.RumbleSecret }
            })
            .OnFailure((_, response) =>
            {
                Log.Error(Owner.Will, "Portal token generation failed.", data: new
                {
                    Response = response.AsGenericData
                });
            })
            .Post(out GenericData json, out int code);

        if (!code.Between(200, 299))
        {
            throw new PlatformException("Portal token generation failed.");
        }

        return json.Require<GenericData>("authorization").Require<string>("token");
    }
}