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
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/mail/admin/global/messages";
        
        ViewData["Success"] = "";
        ViewData["Exist"] = "";
        
        _apiService
            .Request(requestUrl)
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
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/mail/admin/global/messages";
        
        ViewData["Exist"] = "";
        
        try
        {
            List<Attachment> attachmentsList = ParseMessageData.ParseAttachments(attachments);
            icon = ParseMessageData.ParseEmpty(icon);
            banner = ParseMessageData.ParseEmpty(banner);
            long expirationUnix = ParseMessageData.ParseDateTime(expiration);
            long visibleFromUnix = ParseMessageData.ParseDateTime(visibleFrom);
            long? forAccountsBeforeUnix = ParseMessageData.ParseDateTime(forAccountsBefore);

            if (forAccountsBeforeUnix == 0)
            {
                forAccountsBeforeUnix = null;
            }
            
            GlobalMessage newGlobal = new GlobalMessage(subject: subject, body: body, attachments: attachmentsList,
                expiration: expirationUnix, visibleFrom: visibleFromUnix, icon: icon, banner: banner,
                status: Message.StatusType.UNCLAIMED, internalNote: internalNote, forAccountsBefore: forAccountsBeforeUnix);

            _apiService
                .Request(requestUrl + "/send")
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
            .Request(requestUrl)
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
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/mail/admin/messages/send";

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
                .Request(requestUrl)
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

    [Route("edit")]
    public async void Edit(string messageId, string subject, string body, string attachments, string visibleFrom, string expiration,
        string icon, string banner, string status, string internalNote, string forAccountsBefore)
    {
        string token = _dynamicConfigService.GameConfig.Require<string>("mailToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/mail/admin/global/messages/edit";
        
        ViewData["Exist"] = "";

        try
        {
            string oldSubject = TempData["EditSubject"] as string;
            string oldBody = TempData["EditBody"] as string;
            List<Attachment> oldAttachments = TempData.Get<List<Attachment>>("EditAttachments");
            long? oldVisibleFromData = TempData["EditVisibleFrom"] as long?;
            long oldVisibleFrom = oldVisibleFromData ?? default(long);
            long? oldExpirationData = TempData["EditExpiration"] as long?;
            long oldExpiration = oldExpirationData ?? default(long);
            string oldInternalNote = TempData["EditInternalNote"] as string;
            long? oldForAccountsBefore = TempData["EditForAccountsBefore"] as long?;

            if (subject == "")
            {
                subject = oldSubject;
            }
            
            if (body == "")
            {
                body = oldBody;
            }
            
            List<Attachment> attachmentsList = new List<Attachment>();
            if (attachments == "")
            {
                attachmentsList = oldAttachments;
            }
            else
            {
                attachmentsList = ParseMessageData.ParseAttachments(attachments);
            }
            
            icon = ParseMessageData.ParseEmpty(icon);
            
            banner = ParseMessageData.ParseEmpty(banner);

            long expirationUnix = 0;
            if (expiration == "")
            {
                expirationUnix = oldExpiration;
            }
            else
            {
                expirationUnix = ParseMessageData.ParseDateTime(expiration);
            }

            long visibleFromUnix = 0;
            if (visibleFrom == "")
            {
                visibleFromUnix = oldVisibleFrom;
            }
            else
            {
                visibleFromUnix = ParseMessageData.ParseDateTime(visibleFrom);
            }

            if (internalNote == "")
            {
                internalNote = oldInternalNote;
            }

            long? forAccountsBeforeUnix = null;
            if (forAccountsBefore == "")
            {
                forAccountsBeforeUnix = oldForAccountsBefore;
            }
            else
            {
                forAccountsBeforeUnix = ParseMessageData.ParseDateTime(forAccountsBefore);
            }
            
            _apiService
                .Request(requestUrl)
                .AddAuthorization(token)
                .SetPayload(new GenericData
                {
                    {"messageId", messageId},
                    {"subject", subject},
                    {"body", body},
                    {"attachments", attachmentsList},
                    {"expiration", expirationUnix},
                    {"visibleFrom", visibleFromUnix},
                    {"icon", icon},
                    {"banner", banner},
                    {"status", status},
                    {"internalNote", internalNote},
                    {"forAccountsBefore", forAccountsBeforeUnix}
                })
                .OnSuccess(((sender, apiResponse) =>
                {
                    ViewData["Success"] = "Successfully edited message.";
                    Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
                }))
                .OnFailure(((sender, apiResponse) =>
                {
                    ViewData["Success"] = "Failed to edit message.";
                    Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                    {
                        Response = apiResponse
                    });
                }))
                .Patch(out GenericData editResponse, out int editCode);
        }
        catch (Exception e)
        {
            ViewData["Success"] = "Failed to edit message. Some fields may be malformed.";
            Log.Error(owner: Owner.Nathan, message: "Error occurred when editing global message.", data: e.Message);
        }
    }
    
    [Route("delete")]
    public async void Delete(string id)
    {
        string token = _dynamicConfigService.GameConfig.Require<string>("mailToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/mail/admin/global/messages/expire";
        
        ViewData["Exist"] = "";
        
        try
        {
            _apiService
                .Request(requestUrl)
                .AddAuthorization(token)
                .SetPayload(new GenericData
                {
                    {"messageId", id}
                })
                .OnSuccess(((sender, apiResponse) =>
                {
                    ViewData["Success"] = "Successfully deleted message.";
                    Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
                }))
                .OnFailure(((sender, apiResponse) =>
                {
                    ViewData["Success"] = "Failed to delete message.";
                    Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                    {
                        Response = apiResponse
                    });
                }))
                .Patch(out GenericData deleteResponse, out int deleteCode);
        }
        catch (Exception e)
        {
            ViewData["Success"] = "Failed to delete message.";
            Log.Error(owner: Owner.Nathan, message: "Error occurred when deleting global message.", data: e.Message);
        }
    }

    [Route("showEdit")]
    public IActionResult ShowEditOverlay(string id, string subject, string body, List<Attachment> attachments, string visibleFrom, string expiration, string icon, string banner, string internalNote, string forAccountsBefore)
    {
        TempData["EditId"] = id;
        TempData["EditSubject"] = subject;
        TempData["EditBody"] = body;
        //TempData["EditAttachments"] = attachments;
        TempData.Put("EditAttachments", attachments);
        TempData["EditVisibleFrom"] = visibleFrom;
        TempData["EditExpiration"] = expiration;
        TempData["EditInternalNote"] = internalNote;
        TempData["EditForAccountsBefore"] = forAccountsBefore;
        TempData["VisibleOverlay"] = true;
        TempData["VisibleEdit"] = true;
        TempData["VisibleDelete"] = null;
        return RedirectToAction("Global");
    }
    
    [Route("showDelete")]
    public IActionResult ShowDeleteOverlay(string id)
    {
        TempData["DeleteId"] = id;
        TempData["VisibleOverlay"] = true;
        TempData["VisibleEdit"] = null;
        TempData["VisibleDelete"] = true;
        return RedirectToAction("Global");
    }

    [Route("hideOverlay")]
    public IActionResult HideOverlay()
    {
        TempData["EditId"] = null;
        TempData["DeleteId"] = null;
        TempData["VisibleOverlay"] = null;
        TempData["VisibleEdit"] = null;
        TempData["VisibleDelete"] = null;
        return RedirectToAction("Global");
    }
    
    [Route("health")]
    public override ActionResult HealthCheck() => Ok(_apiService.HealthCheckResponseObject, _dynamicConfigService.HealthCheckResponseObject);
}