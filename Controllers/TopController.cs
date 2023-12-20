using System.Linq;
using Dmz.Interop;
using Dmz.Models.Portal;
using Dmz.Services;
using Dmz.Utilities;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Models.Alerting;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Data;

namespace Dmz.Controllers;

[ApiController, Route(template: "dmz")]
public class TopController : DmzController
{
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

        AuditLog[] logs = AccountService.Instance.GetActivityLogs();

        return Optional<bool>("messageOnly")
            ? Ok(new RumbleJson { { "messages", logs.Select(log => $"{log.Time} | {log.Who.PadLeft(totalWidth: 40, paddingChar: ' ')} | {log.Message ?? $"{log.Method} {log.Endpoint} {log.ResultCode}"}") } })
            : Ok(new RumbleJson { { "logs", logs } });
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