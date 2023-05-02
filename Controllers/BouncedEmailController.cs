using System;
using Dmz.Models.Bounces;
using Dmz.Services;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Exceptions.Mongo;
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

    [HttpGet, Route("stats")]
    public ActionResult GetEmailBounceStatus()
    {
        #if RELEASE
        Require(Permissions.Portal.ViewBouncedEmails);
        #endif
        
        string email = Require<string>("email");

        return Ok(_bouncer.GetBounceSummary(email) ?? new BounceData
        {
            Email = email
        });
    }
    
    [HttpGet, Route("banList")]
    public ActionResult GetBannedEmailList()
    {
        #if RELEASE
        Require(Permissions.Portal.ViewBouncedEmails);
        #endif

        long timestamp = Optional<long?>("lastBounceTime") ?? Timestamp.UnixTime;

        return Ok(new RumbleJson
        {
            { "banList", _bouncer.GetBannedList(timestamp) }
        });
    }

    [HttpPatch, Route("unban")]
    public ActionResult Unban()
    {
        #if RELEASE
        Require(Permissions.Portal.UnbanBouncedAddress);
        #endif

        string email = Require<string>("email");

        return _bouncer.Unban(email)
            ? Ok()
            : throw new RecordNotFoundException(_bouncer.CollectionName, "Banned email address not found", data: new RumbleJson
            {
                { "address", email }
            });
    }
}