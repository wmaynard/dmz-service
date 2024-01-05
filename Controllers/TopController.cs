using System;
using System.Linq;
using Dmz.Interop;
using Dmz.Models.Portal;
using Dmz.Services;
using Dmz.Utilities;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Models.Alerting;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Data;

namespace Dmz.Controllers;

[ApiController, Route(template: "dmz")]
public class TopController : DmzController
{
    #pragma warning disable
    private readonly ActivityLogService _activities;
    #pragma warning restore
    
    [HttpGet, Route("env"), RequireAuth]
    public ActionResult GetEnvironment() => Ok(new RumbleJson
    {
        { "gameSecret", PlatformEnvironment.GameSecret },
        { "rumbleSecret", PlatformEnvironment.RumbleSecret }
    });

    [HttpGet, Route("activity"), RequireAuth(AuthType.ADMIN_TOKEN)]
    public ActionResult GetActivityLog() // TODO: Fix the performance here
    {
        Require(Permissions.Portal.ViewActivityLogs);

        bool messageOnly = Optional<bool>("messageOnly");
        int size = Math.Max(100, Math.Min(10, Optional<int>("pageSize")));
        int page = Optional<int>("page");
        string accountId = Optional<string>(TokenInfo.FRIENDLY_KEY_ACCOUNT_ID);

        AuditLog[] logs = _activities.Page(size, page, accountId, out long remaining);
        
        RumbleJson output = new()
        {
            { "remaining", remaining }
        };

        if (messageOnly)
            output["messages"] = logs.Select(log => $"{log.CreatedOn} | {log.Who.Email.PadLeft(totalWidth: 40, paddingChar: ' ')} | {log.Message ?? $"{log.Method} {log.Endpoint} {log.ResultCode}"}");
        else
            output["logs"] = logs;

        return Ok(output);
    }

    [HttpPost, Route("alert"), RequireAuth(AuthType.ADMIN_TOKEN)]
    public ActionResult SendEmailAlert()
    {
        string email = Require<string>("email");
        Alert alert = Require<Alert>("alert");

        PlatformAlertEmail.SendAlert(email, alert);
        

        return Ok();
    }
}