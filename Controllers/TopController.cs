using System.Linq;
using Dmz.Models.Portal;
using Dmz.Services;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
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
    public ActionResult GetActivityLog()
    {
        Require(Permissions.Portal.ViewActivityLogs);

        AuditLog[] logs = AccountService.Instance.GetActivityLogs(limit: Optional<int?>("limit"));

        return Optional<bool>("messageOnly")
            ? Ok(new RumbleJson { { "messages", logs.Select(log => $"{log.Time} | {log.Who.PadLeft(totalWidth: 40, paddingChar: ' ')} | {log.Message ?? $"{log.Method} {log.Endpoint} {log.ResultCode}"}") } })
            : Ok(new RumbleJson { { "logs", logs } });
    }
}