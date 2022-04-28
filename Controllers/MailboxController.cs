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

    // Global message routes
    
    // Fetch all global messages
    [Route("global")]
    public async Task<IActionResult> Global()
    {
        string token = _dynamicConfigService.GameConfig.Require<string>("mailToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/mail/admin/global/messages";
        
        TempData["Success"] = "";
        TempData["Failure"] = null;

        ViewData["Today"] = DefaultDateTime.UtcDateTimeString();
        ViewData["Week"] = DefaultDateTime.UtcDateTimeString(days: 7);
        
        _apiService
            .Request(requestUrl)
            .AddAuthorization(token)
            .OnSuccess(((sender, apiResponse) =>
            {
                TempData["Success"] = "Successfully fetched global messages.";
                TempData["Failure"] = null;
                Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                TempData["Success"] = "Failed to fetch global messages.";
                TempData["Failure"] = true;
                Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Get(out GenericData response, out int code);
        
        List<GlobalMessage> globalMessages = response.Require<List<GlobalMessage>>(key: "globalMessages");

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

    // Sending a new global messages then fetching all global messages upon page reload
    // Perhaps can refactor to send and then redirect back to GET /global
    [HttpPost]
    [Route("global")]
    public async Task<IActionResult> Global(string subject, string body, string attachments, string visibleFrom, string expiration,
        string icon, string banner, string internalNote, string forAccountsBefore)
    {
        string token = _dynamicConfigService.GameConfig.Require<string>("mailToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/mail/admin/global/messages";
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
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
                    TempData["Success"] = "Successfully sent message.";
                    TempData["Failure"] = null;
                    Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
                }))
                .OnFailure(((sender, apiResponse) =>
                {
                    TempData["Success"] = "Failed to send message.";
                    TempData["Failure"] = true;
                    Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                    {
                        Response = apiResponse
                    });
                }))
                .Post(out GenericData sendResponse, out int sendCode);
        }
        catch (Exception e)
        {
            TempData["Success"] = "Failed to send message. Some fields may be malformed.";
            TempData["Failure"] = true;
            Log.Error(owner: Owner.Nathan, message: "Error occurred when sending global message.", data: e.Message);
        }
        
        _apiService
            .Request(requestUrl)
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
        
        ViewData["Today"] = DefaultDateTime.UtcDateTimeString();
        ViewData["Week"] = DefaultDateTime.UtcDateTimeString(days: 7);

        ViewData["ActiveGlobalMessages"] = activeGlobalMessagesList;
        ViewData["ExpiredGlobalMessages"] = expiredGlobalMessagesList;
        
        return View();
    }

    // Edit global messages
    [Route("edit")]
    public async Task<IActionResult> Edit(string messageId, string subject, string body, string attachments, string visibleFrom, string expiration,
        string icon, string banner, string status, string internalNote, string forAccountsBefore)
    {
        string token = _dynamicConfigService.GameConfig.Require<string>("mailToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/mail/admin/global/messages/edit";
        
        TempData["Success"] = "";
        TempData["Failure"] = null;

        try
        {
            string oldSubject = TempData["EditSubject"] as string;
            string oldBody = TempData["EditBody"] as string;
            List<Attachment> oldAttachments = TempData.Get<List<Attachment>>("EditAttachments");
            string oldVisibleFromData = TempData["EditVisibleFrom"] as string;
            long oldVisibleFrom = long.Parse(oldVisibleFromData);
            string oldExpirationData = TempData["EditExpiration"] as string;
            long oldExpiration = long.Parse(oldExpirationData);
            string oldInternalNote = TempData["EditInternalNote"] as string;
            long? oldForAccountsBefore = TempData["EditForAccountsBefore"] as long?;

            if (subject == null)
            {
                subject = oldSubject;
            }
            
            if (body == null)
            {
                body = oldBody;
            }
            
            List<Attachment> attachmentsList = new List<Attachment>();
            if (attachments == null)
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
            if (expiration == null)
            {
                expirationUnix = oldExpiration;
            }
            else
            {
                expirationUnix = ParseMessageData.ParseDateTime(expiration);
            }

            long visibleFromUnix = 0;
            if (visibleFrom == null)
            {
                visibleFromUnix = oldVisibleFrom;
            }
            else
            {
                visibleFromUnix = ParseMessageData.ParseDateTime(visibleFrom);
            }

            if (internalNote == null)
            {
                internalNote = oldInternalNote;
            }

            long? forAccountsBeforeUnix = null;
            if (forAccountsBefore == null)
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
                    TempData["Success"] = "Successfully edited message.";
                    TempData["Failure"] = null;
                    Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
                }))
                .OnFailure(((sender, apiResponse) =>
                {
                    TempData["Success"] = "Failed to edit message.";
                    TempData["Failure"] = true;
                    Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                    {
                        Response = apiResponse
                    });
                }))
                .Patch(out GenericData editResponse, out int editCode);
        }
        catch (Exception e)
        {
            TempData["Success"] = "Failed to edit message. Some fields may be malformed.";
            TempData["Failure"] = true;
            Log.Error(owner: Owner.Nathan, message: "Error occurred when editing global message.", data: e.Message);
        }

        return RedirectToAction("Global");
    }
    
    // Delete global messages
    [Route("delete")]
    public async Task<IActionResult> Delete(string id)
    {
        string token = _dynamicConfigService.GameConfig.Require<string>("mailToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/mail/admin/global/messages/expire";
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
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
                    TempData["Success"] = "Successfully deleted message.";
                    TempData["Failure"] = null;
                    Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
                }))
                .OnFailure(((sender, apiResponse) =>
                {
                    TempData["Success"] = "Failed to delete message.";
                    TempData["Failure"] = true;
                    Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                    {
                        Response = apiResponse
                    });
                }))
                .Patch(out GenericData deleteResponse, out int deleteCode);
        }
        catch (Exception e)
        {
            TempData["Success"] = "Failed to delete message.";
            TempData["Failure"] = true;
            Log.Error(owner: Owner.Nathan, message: "Error occurred when deleting global message.", data: e.Message);
        }

        return RedirectToAction("Global");
    }

    // Used to display edit form and overlay upon clicking edit on a global message
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
    
    // Used to display delete confirmation and overlay upon clicking edit on a global message
    [Route("showDelete")]
    public IActionResult ShowDeleteOverlay(string id)
    {
        TempData["DeleteId"] = id;
        TempData["VisibleOverlay"] = true;
        TempData["VisibleEdit"] = null;
        TempData["VisibleDelete"] = true;
        return RedirectToAction("Global");
    }

    // Used to hide edit, delete, and the overlay
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

    // Not currently used. In the future, change to toggle timestamp inputs
    [Route("toggleInput")]
    public IActionResult ToggleInput()
    {
        if (TempData["ToggleInput"] != null)
        {
            TempData["ToggleInput"] = null;
        }
        else
        {
            TempData["ToggleInput"] = true;
        }

        return RedirectToAction("Global");
    }
    
    
    
    // Group / Direct message routes
    
    // View form to send direct messages
    [Route("group")]
    public async Task<IActionResult> Group()
    {
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        ViewData["Today"] = DefaultDateTime.UtcDateTimeString();
        ViewData["Week"] = DefaultDateTime.UtcDateTimeString(days: 7);
        return View();
    }

    // Send direct messages
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
                    TempData["Success"] = "Successfully sent message.";
                    TempData["Failure"] = null;
                    Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
                }))
                .OnFailure(((sender, apiResponse) =>
                {
                    TempData["Success"] = "Failed to send message.";
                    TempData["Failure"] = true;
                    Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                    {
                        Response = apiResponse
                    });
                }))
                .Post(out GenericData sendResponse, out int sendCode);
        }
        catch (Exception e)
        {
            TempData["Success"] = "Failed to send message. Some fields may be malformed.";
            TempData["Failure"] = true;
            Log.Error(owner: Owner.Nathan, message: "Error occurred when sending group message.", data: e.Message);
        }
        
        ViewData["Today"] = DefaultDateTime.UtcDateTimeString();
        ViewData["Week"] = DefaultDateTime.UtcDateTimeString(days: 7);

        return View();
    }
    
    
    
    // Player inbox routes

    // Display search form for looking at player inbox
    [Route("inbox")]
    public async Task<IActionResult> Inbox()
    {
        return View();
    }
    
    // Fetches the inbox for the given accountId
    [HttpPost]
    [Route("inbox")]
    public async Task<IActionResult> Inbox(string accountId)
    {
        string token = _dynamicConfigService.GameConfig.Require<string>("mailToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/mail/admin/inbox";
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        _apiService
            .Request(requestUrl)
            .AddAuthorization(token)
            .SetPayload(new GenericData
            {
                {"accountId", accountId}
            })
            .OnSuccess(((sender, apiResponse) =>
            {
                TempData["Success"] = "Successfully fetched inbox messages.";
                TempData["Failure"] = null;
                Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                TempData["Success"] = "Failed to fetch inbox messages.";
                TempData["Failure"] = true;
                Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Post(out GenericData response, out int code);

        Inbox inbox = response.Require<Inbox>(key: "inbox");
        
        List<Message> activeMessagesList = inbox.Messages;
        List<Message> historyMessagesList = inbox.History;

        ViewData["InboxMessages"] = activeMessagesList;
        ViewData["HistoryMessages"] = historyMessagesList;
        
        return View();
    }
    
    [Route("health")]
    public override ActionResult HealthCheck() => Ok(_apiService.HealthCheckResponseObject, _dynamicConfigService.HealthCheckResponseObject);
}