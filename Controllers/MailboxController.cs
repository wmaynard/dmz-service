using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        
        ViewData["Success"] = "";
        ViewData["Exist"] = "";
        
        _apiService
            .Request($"https://dev.nonprod.tower.cdrentertainment.com/mail/admin/global/messages")
            .AddAuthorization(token)
            .OnSuccess(((sender, apiResponse) =>
            {
                ViewData["Exist"] = "Successfully fetched global messages.";
                Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                ViewData["Exist"] = "Failed to fetch global messages.";
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
        
        ViewData["Exist"] = "";
        
        try
        {
            List<Attachment> attachmentsList = ParseMessageData.ParseAttachments(attachments);
            icon = ParseMessageData.ParseEmpty(icon);
            banner = ParseMessageData.ParseEmpty(banner);
            long expirationUnix = ParseMessageData.ParseDateTime(expiration);
            long visibleFromUnix = ParseMessageData.ParseDateTime(visibleFrom);
            long? forAccountsBeforeUnix = ParseMessageData.ParseDateTime(forAccountsBefore);
            
            GlobalMessage newGlobal = new GlobalMessage(subject: subject, body: body, attachments: attachmentsList,
                expiration: expirationUnix, visibleFrom: visibleFromUnix, icon: icon, banner: banner,
                status: Message.StatusType.UNCLAIMED, internalNote: internalNote, forAccountsBefore: forAccountsBeforeUnix);

            _apiService
                .Request($"https://dev.nonprod.tower.cdrentertainment.com/mail/admin/global/messages/send")
                //.Request($"https://localhost:5071/mail/admin/messages/send") // local
                .AddAuthorization(token)
                .SetPayload(new GenericData
                {
                    {"globalMessage", newGlobal}
                })
                .OnSuccess(((sender, apiResponse) =>
                {
                    ViewData["Success"] = "Successfully sent message.";
                    Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
                }))
                .OnFailure(((sender, apiResponse) =>
                {
                    ViewData["Success"] = "Failed to send message.";
                    Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                    {
                        Response = apiResponse
                    });
                }))
                .Post(out GenericData sendResponse, out int sendCode);
        }
        catch (Exception e)
        {
            ViewData["Success"] = "Failed to send message. Some fields may be malformed.";
            Log.Error(owner: Owner.Nathan, message: "Error occurred when sending global message.", data: e.Message);
        }

        _apiService
            .Request($"https://dev.nonprod.tower.cdrentertainment.com/mail/admin/global/messages")
            .AddAuthorization(token)
            .OnSuccess(((sender, apiResponse) =>
            {
                ViewData["Exist"] = "Successfully fetched global messages.";
                Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                ViewData["Exist"] = "Failed to fetch global messages.";
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
    
    [Route("group")]
    public async Task<IActionResult> Group()
    {
        ViewData["Success"] = "";
        return View();
    }

    [HttpPost]
    [Route("group")]
    public async Task<IActionResult> Group(string playerIds, string subject, string body, string attachments,
        string visibleFrom, string expiration, string icon, string banner, string internalNote)
    {
        string token = _dynamicConfigService.GameConfig.Require<string>("mailToken");
        
        try
        {
            List<string> playerIdsList = ParseMessageData.ParseIds(playerIds);
            List<Attachment> attachmentsList = ParseMessageData.ParseAttachments(attachments);
            icon = ParseMessageData.ParseEmpty(icon);
            banner = ParseMessageData.ParseEmpty(banner);
            long expirationUnix = ParseMessageData.ParseDateTime(expiration);
            long visibleFromUnix = ParseMessageData.ParseDateTime(visibleFrom);
            
            Message newMessage = new Message(subject: subject, body: body, attachments: attachmentsList,
                expiration: expirationUnix, visibleFrom: visibleFromUnix, icon: icon, banner: banner,
                status: Message.StatusType.UNCLAIMED, internalNote: internalNote);

            _apiService
                .Request($"https://dev.nonprod.tower.cdrentertainment.com/mail/admin/messages/send")
                //.Request($"https://localhost:5071/mail/admin/messages/send") // local
                .AddAuthorization(token)
                .SetPayload(new GenericData
                {
                    {"accountIds", playerIdsList},
                    {"message", newMessage}
                })
                .OnSuccess(((sender, apiResponse) =>
                {
                    ViewData["Success"] = "Successfully sent message.";
                    Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
                }))
                .OnFailure(((sender, apiResponse) =>
                {
                    ViewData["Success"] = "Failed to send message.";
                    Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                    {
                        Response = apiResponse
                    });
                }))
                .Post(out GenericData sendResponse, out int sendCode);
        }
        catch (Exception e)
        {
            ViewData["Success"] = "Failed to send message. Some fields may be malformed.";
            Log.Error(owner: Owner.Nathan, message: "Error occurred when sending group message.", data: e.Message);
        }

        return View();
    }

    [Route("health")]
    public override ActionResult HealthCheck() => Ok(_apiService.HealthCheckResponseObject, _dynamicConfigService.HealthCheckResponseObject);
}