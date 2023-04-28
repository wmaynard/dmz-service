using System;
using Dmz.Services;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Data;

namespace Dmz.Controllers;

[Route("dmz/bounces")]
public class BouncedEmailController : DmzController
{
    #pragma warning disable
    private readonly BounceHandlerService _bouncer;
    #pragma warning restore
    
    [HttpGet, Route("valid"), NoAuth]
    public ActionResult BanStatus()
    {
        string email = Require<string>("email");

        _bouncer.EnsureNotBanned(email);

        return Ok();
    }

    [HttpGet, Route("status")]
    public ActionResult GetEmailBounceStatus()
    {
        Require(Permissions.Portal.ViewBouncedEmails);
        
        string email = Require<string>("email");

        return Ok(_bouncer.GetBounceSummary(email));
    }
    
    [HttpGet, Route("banList")]
    public ActionResult GetBannedEmailList()
    {
        Require(Permissions.Portal.ViewBouncedEmails);

        long timestamp = Optional<long?>("lastBounceTime") ?? Timestamp.UnixTime;

        return Ok(new RumbleJson
        {
            { "banList", _bouncer.GetBannedList(timestamp) }
        });
    }
}