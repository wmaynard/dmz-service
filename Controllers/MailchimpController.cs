using System;
using Dmz.Interop;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;

namespace Dmz.Controllers;

[Route("/dmz/mailchimp"), RequireAuth]
public class MailchimpController : PlatformController
{
    #pragma warning disable
    private readonly MailchimpService _mailchimp;
    private readonly DynamicConfig _config;
#pragma warning restore
    
    [HttpGet, Route("claim"), NoAuth]
    public ActionResult ClaimReward()
    {
        string accountId = null;
        string templateId = null;

        string successUrl = _config
            .Optional<string>("mailchimpSuccessPage")
            ?.Replace("{reason}", "foo") // TODO: Any replacements?
            ?? "https://towersandtitans.com/";
        string failureUrl = _config
            .Optional<string>("mailchimpSuccessPage")
            ?.Replace("{reason}", "foo") // TODO: Any replacements?
            ?? "https://towersandtitans.com/";;
        
        try
        {
            accountId = Require<string>(TokenInfo.FRIENDLY_KEY_ACCOUNT_ID); 
            templateId = Require<string>("emailId");
        
            _mailchimp.ClaimEmail(accountId, templateId);
            return Redirect(successUrl);
        }
        catch (Exception e)
        {
            Log.Error(Owner.Will, "Unable to issue reward to an account from Mailchimp campaign", data: new
            {
                accountId = accountId,
                templateId = templateId
            }, exception: e);
        }
        return Redirect(failureUrl);
    }

    [HttpPatch, Route("subscription")]
    public ActionResult ModifySubscription()
    {
        if (string.IsNullOrWhiteSpace(Token?.Email))
            throw new PlatformException("Email unavailable in token; Mailchimp requires an email to unsubscribe via API", code: ErrorCode.InvalidRequestData);
        
        bool subscribed = Require<bool>("subscribed");
        
        _mailchimp.AlterSubscription(Token, subscribed);

        return Ok();
    }

    [HttpGet, Route("status")]
    public ActionResult GetStatus() => Ok(_mailchimp.GetMember(Token.AccountId));
}