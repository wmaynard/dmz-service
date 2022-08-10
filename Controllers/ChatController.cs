using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Models;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Authorize]
[Route("/portal/chat")]
public class ChatController : PortalController
{
  private readonly DynamicConfigService _dynamicConfigService;
  private readonly AccountService       _accountService;

  [Route("announcements")]
  public async Task<IActionResult> Announcements()
  {
    // Checking access permissions
    if (!UserPermissions.ViewChat)
    {
      return View("Error");
    }
    
    TempData["Success"] = "";
    TempData["Failure"] = null;
  
    _apiService
        .Request(PlatformEnvironment.Url("/chat/admin/messages/sticky"))
        .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
        .OnSuccess((sender, apiResponse) =>
        {
            TempData["Success"] = "Successfully fetched chat announcements.";
            TempData["Failure"] = null;
            Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
        })
        .OnFailure((sender, apiResponse) =>
        {
            TempData["Success"] = "Failed to fetch chat announcements.";
            TempData["Failure"] = true;
            Log.Error(owner: Owner.Nathan, message: "Request to chat-service failed.", data: new
            {
                Response = apiResponse
            });
        })
        .Get(out GenericData response, out int code);

    List<Announcement> announcements = new List<Announcement>();

    try
    {
        announcements = response.Require<List<Announcement>>(key: "stickies");
    }
    catch (Exception e)
    {
        Log.Error(owner: Owner.Nathan, message: "Failed to parse response from chat-service.", data: e);
    }
    
    ViewData["Announcements"] = announcements;
    
    return View();
  }
  
  [Route("reports")]
  public async Task<IActionResult> Reports()
  {
    // Checking access permissions
    if (!UserPermissions.ViewChat)
    {
      return View("Error");
    }
    
    TempData["Success"] = "";
    TempData["Failure"] = null;
  
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/ban/list")) // TODO double check if reports or bans
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .OnSuccess((sender, apiResponse) =>
                 {
                   TempData["Success"] = "Successfully fetched chat bans.";
                   TempData["Failure"] = null;
                   Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                 })
      .OnFailure((sender, apiResponse) =>
                 {
                   TempData["Success"] = "Failed to fetch chat bans.";
                   TempData["Failure"] = true;
                   Log.Error(owner: Owner.Nathan, message: "Request to chat-service failed.", data: new
                                                                                                    {
                                                                                                      Response = apiResponse
                                                                                                    });
                 })
      .Get(out GenericData response, out int code);

    List<ChatBan> chatBans = new List<ChatBan>();

    try
    {
      chatBans = response.Require<List<ChatBan>>(key: "bans");
    }
    catch (Exception e)
    {
      Log.Error(owner: Owner.Nathan, message: "Failed to parse response from chat-service.", data: e);
    }
    
    ViewData["ChatBans"] = chatBans;
  
    return View();
  }

  [Route("player")]
  public async Task<IActionResult> Player()
  {
    // Checking access permissions
    if (!UserPermissions.ViewChat)
    {
      return View("Error");
    }
    
    
    
    return View();
  }
}