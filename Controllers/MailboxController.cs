using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;

namespace TowerPortal.Controllers;

[Authorize]
[Route("mailbox")]
public class MailboxController : PlatformController
{
    private readonly ApiService _apiService;
    private readonly DynamicConfigService _dynamicConfigService;

    [Route("global")]
    public async Task<IActionResult> Global()
    {
        string token = _dynamicConfigService.GameConfig.Require<string>("mailToken");
        
        _apiService
            .Request($"https://dev.nonprod.tower.cdrentertainment.com/mail/admin/global/messages")
            .AddAuthorization(token)
            .OnSuccess(((sender, apiResponse) =>
            {
                Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Get(out GenericData response, out int code);
        
        //GenericData.require<model>("key")
        List<GlobalMessage> globalMessages = response.Require<List<GlobalMessage>>("globalMessages");

        List<GlobalMessage> activeGlobalMessagesList = new List<GlobalMessage>();
        List<GlobalMessage> expiredGlobalMessagesList = new List<GlobalMessage>();

        foreach (GlobalMessage globalMessage in globalMessages)
        {
            if (!globalMessage.IsExpired)
            {
                activeGlobalMessagesList.Add(globalMessage);
            } else if (globalMessage.IsExpired)
            {
                expiredGlobalMessagesList.Add(globalMessage);
            }
        }

        ViewData["ActiveGlobalMessages"] = activeGlobalMessagesList;
        ViewData["ExpiredGlobalMessages"] = expiredGlobalMessagesList;
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Global(string subject, string body, List<Attachment> attachments, long visibleFrom, long expiration,
        string icon, string banner, string internalNote, long forAccountsBefore)
    {
        Log.Local(owner: Owner.Nathan, message: "post request");
        //ViewData["Request"] = new GlobalMessage(subject: subject, body: body, attachments: attachments,
        //    expiration: expiration, visibleFrom: visibleFrom, icon: icon, banner: banner,
        //    status: Message.StatusType.UNCLAIMED, internalNote: internalNote, forAccountsBefore: forAccountsBefore);
        ViewData["Test"] = subject;
        Log.Local(owner: Owner.Nathan, message: "post success");
        return View();
    }

    [Route("health")]
    public override ActionResult HealthCheck() => Ok(_apiService.HealthCheckResponseObject, _dynamicConfigService.HealthCheckResponseObject);
}