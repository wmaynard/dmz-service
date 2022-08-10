using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

    List<ChatBan> chatBansList = new List<ChatBan>();

    try
    {
      chatBansList = response.Require<List<ChatBan>>(key: "bans");
    }
    catch (Exception e)
    {
      Log.Error(owner: Owner.Nathan, message: "Failed to parse response from chat-service.", data: e);
    }
    
    ViewData["ChatBans"] = chatBansList;
  
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
  
  [HttpPost]
  [Route("player")]
  public async Task<IActionResult> Player(string accountId)
  {
    // Checking access permissions
    if (!UserPermissions.ViewChat)
    {
      return View("Error");
    }
    
    TempData["Success"] = "";
    TempData["Failure"] = null;
  
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/playerDetails"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"aid", accountId}
                  })
      .OnSuccess((sender, apiResponse) =>
                 {
                   TempData["Success"] = "Successfully fetched chat player details.";
                   TempData["Failure"] = null;
                   Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                 })
      .OnFailure((sender, apiResponse) =>
                 {
                   TempData["Success"] = "Failed to fetch chat player details.";
                   TempData["Failure"] = true;
                   Log.Error(owner: Owner.Nathan, message: "Request to chat-service failed.", data: new
                                                                                              {
                                                                                                Response = apiResponse
                                                                                              });
                 })
      .Post(out GenericData response, out int code);

    List<ChatBan> chatBansList = new List<ChatBan>();
    List<ChatReport> chatReportsList = new List<ChatReport>();

    try
    {
      chatBansList = response.Require<List<ChatBan>>(key: "bans");
      chatReportsList = response.Require<List<ChatReport>>(key: "reports");
    }
    catch (Exception e)
    {
      Log.Error(owner: Owner.Nathan, message: "Failed to parse response from chat-service.", data: e);
    }

    ViewData["AccountId"] = accountId;

    ViewData["ChatBans"] = chatBansList;
    ViewData["ChatReports"] = chatReportsList;

    return View();
  }

  [HttpPost]
  [Route("ban")]
  public async Task<IActionResult> Ban(string accountId, string reason, long? duration)
  {
    // Checking access permissions
    if (!UserPermissions.ViewChat || !UserPermissions.EditChat)
    {
      return View("Error");
    }
    
    TempData["Success"] = "";
    TempData["Failure"] = null;
        
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/ban/player"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"aid", accountId},
                    {"reason", reason},
                    {"duration", duration}
                  })
      .OnSuccess(((sender, apiResponse) =>
                  {
                    TempData["Success"] = "Successfully banned player.";
                    TempData["Failure"] = null;
                    Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                  }))
      .OnFailure(((sender, apiResponse) =>
                  {
                    TempData["Success"] = "Failed to ban player.";
                    TempData["Failure"] = true;
                    Log.Error(Owner.Nathan, "Request to chat-service failed.", data: new
                                                                                     {
                                                                                       Response = apiResponse
                                                                                     });
                  }))
      .Post(out GenericData response, out int code);
  
    return RedirectToAction("Player", new { accountId = accountId });
  }
  
  [HttpPost]
  [Route("unban")]
  public async Task<IActionResult> Unban(string accountId, string banId)
  {
    // Checking access permissions
    if (!UserPermissions.ViewChat || !UserPermissions.EditChat)
    {
      return View("Error");
    }
    
    TempData["Success"] = "";
    TempData["Failure"] = null;
        
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/ban/lift"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"banId", banId}
                  })
      .OnSuccess(((sender, apiResponse) =>
                  {
                    TempData["Success"] = "Successfully unbanned player.";
                    TempData["Failure"] = null;
                    Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                  }))
      .OnFailure(((sender, apiResponse) =>
                  {
                    TempData["Success"] = "Failed to unban player.";
                    TempData["Failure"] = true;
                    Log.Error(Owner.Nathan, "Request to chat-service failed.", data: new
                                                                                     {
                                                                                       Response = apiResponse
                                                                                     });
                  }))
      .Post(out GenericData response, out int code);
  
    return RedirectToAction("Player", new { accountId = accountId });
  }
  
  [HttpPost]
  [Route("ignore")]
  public async Task<IActionResult> Ignore(string accountId, string reportId)
  {
    // Checking access permissions
    if (!UserPermissions.ViewChat || !UserPermissions.EditChat)
    {
      return View("Error");
    }
    
    TempData["Success"] = "";
    TempData["Failure"] = null;
        
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/reports/ignore"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"reportId", reportId}
                  })
      .OnSuccess(((sender, apiResponse) =>
                  {
                    TempData["Success"] = "Successfully ignored report.";
                    TempData["Failure"] = null;
                    Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                  }))
      .OnFailure(((sender, apiResponse) =>
                  {
                    TempData["Success"] = "Failed to ignore report.";
                    TempData["Failure"] = true;
                    Log.Error(Owner.Nathan, "Request to chat-service failed.", data: new
                                                                                     {
                                                                                       Response = apiResponse
                                                                                     });
                  }))
      .Post(out GenericData response, out int code);
  
    return RedirectToAction("Player", new { accountId = accountId });
  }
  
  [HttpPost]
  [Route("delete")]
  public async Task<IActionResult> Delete(string accountId, string reportId)
  {
    // Checking access permissions
    if (!UserPermissions.ViewChat || !UserPermissions.EditChat)
    {
      return View("Error");
    }
    
    TempData["Success"] = "";
    TempData["Failure"] = null;
        
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/reports/delete"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"reportId", reportId}
                  })
      .OnSuccess(((sender, apiResponse) =>
                  {
                    TempData["Success"] = "Successfully deleted report.";
                    TempData["Failure"] = null;
                    Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                  }))
      .OnFailure(((sender, apiResponse) =>
                  {
                    TempData["Success"] = "Failed to delete report.";
                    TempData["Failure"] = true;
                    Log.Error(Owner.Nathan, "Request to chat-service failed.", data: new
                                                                                       {
                                                                                         Response = apiResponse
                                                                                       });
                  }))
      .Post(out GenericData response, out int code);
  
    return RedirectToAction("Player", new { accountId = accountId });
  }
}