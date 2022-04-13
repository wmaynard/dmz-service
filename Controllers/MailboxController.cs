using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;
using TowerPortal.Utilities;

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
    [Route("global")]
    public async Task<IActionResult> Global(string subject, string body, string attachments, string visibleFrom, string expiration,
        string icon, string banner, string internalNote, string forAccountsBefore)
    {
        string token = _dynamicConfigService.GameConfig.Require<string>("mailToken");
        
        List<Attachment> attachmentsList = null;
        long expirationUnix = 0;
        long visibleFromUnix = 0;
        long? forAccountsBeforeUnix = null;
        
        try
        {
            attachmentsList = ParseMessageData.ParseAttachments(attachments);
            icon = ParseMessageData.ParseEmpty(icon);
            banner = ParseMessageData.ParseEmpty(banner);
            expirationUnix = ParseMessageData.ParseDateTime(expiration);
            visibleFromUnix = ParseMessageData.ParseDateTime(visibleFrom);
            forAccountsBeforeUnix = ParseMessageData.ParseDateTime(forAccountsBefore);
        }
        catch (Exception e)
        {
            Log.Error(owner: Owner.Nathan, message: "Error occurred when sending global message.", data: e.Message);
        }

        GlobalMessage newGlobal = new GlobalMessage(subject: subject, body: body, attachments: attachmentsList,
            expiration: expirationUnix, visibleFrom: visibleFromUnix, icon: icon, banner: banner,
            status: Message.StatusType.UNCLAIMED, internalNote: internalNote, forAccountsBefore: forAccountsBeforeUnix);

        _apiService
            .Request($"https://dev.nonprod.tower.cdrentertainment.com/mail/admin/global/messages/send")
            .AddAuthorization(token)
            //.SetPayload((GenericData)newGlobal)
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
            .Get(out GenericData sendResponse, out int sendCode);
        
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

    [Route("health")]
    public override ActionResult HealthCheck() => Ok(_apiService.HealthCheckResponseObject, _dynamicConfigService.HealthCheckResponseObject);
}