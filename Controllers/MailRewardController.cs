using System;
using System.Collections.Generic;
using Dmz.Exceptions;
using Dmz.Interop;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Minq;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Common.Utilities.JsonTools;

namespace Dmz.Controllers;

[Route("/dmz/rewards"), RequireAuth]
public class MailRewardController : DmzController
{
    #pragma warning disable
    
    private readonly DynamicConfig _config;
    #pragma warning restore
    
    [HttpGet, Route("claim"), NoAuth]
    public ActionResult ClaimReward()
    {
        string redirectUrl = "https://towersandtitans.com";
        try
        {
            Forward("/mail/claim", out RumbleJson response);

            redirectUrl = response
                .Optional<RumbleJson>("campaignMessage")
                ?.Optional<string>("redirectUrl")
                ?? _config.Optional<string>("mailClaimSuccessPage")
                ?? redirectUrl;

            return Redirect(redirectUrl);
        }
        catch (ForwardingException e)
        {
            Log.Error(Owner.Will, "Unable to forward mail claim request", exception: e);
            
            redirectUrl = _config.Optional<string>("mailClaimFailurePage") ?? redirectUrl;
            return Redirect(redirectUrl);
        }
    }
}