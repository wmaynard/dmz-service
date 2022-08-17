using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Enums;
using TowerPortal.Models.Chat;
using TowerPortal.Services;
using TowerPortal.Utilities;

namespace TowerPortal.Controllers;

[Authorize]
[Route("/portal/chat")]
public class ChatController : PortalController
{
#pragma warning disable CS0649
  private readonly DynamicConfigService _dynamicConfigService;
#pragma warning restore CS0649

  [Route("announcements")]
  public async Task<IActionResult> Announcements()
  {
    // Checking access permissions
    if (!Permissions.Chat.View_Page)
    {
      return View("Error");
    }
    
    ClearStatus();
  
    _apiService
        .Request(PlatformEnvironment.Url("/chat/admin/messages/sticky"))
        .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
        .OnSuccess((sender, apiResponse) =>
        {
            SetStatus("Successfully fetched chat announcements.", RequestStatus.Success);
            Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
        })
        .OnFailure((sender, apiResponse) =>
        {
            SetStatus("Failed to fetch chat announcements.", RequestStatus.Error);
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

  [HttpPost]
  [Route("announcements")]
  public async Task<IActionResult> Announcements(string text, long? durationInSeconds, string expiration, string visibleFrom, string language)
  {
    // Checking access permissions
    if (!Permissions.Chat.View_Page || !Permissions.Chat.Edit)
    {
      return View("Error");
    }
    
    ClearStatus();

    long? expirationUnix = ParseMessageData.ParseDateTime(expiration);
    long? visibleFromUnix = ParseMessageData.ParseDateTime(visibleFrom);
    
    Dictionary<string, dynamic> stickyMessageDict = new Dictionary<string, dynamic>();
    stickyMessageDict.Add("text", text);
    if (durationInSeconds != null)
    {
      stickyMessageDict.Add("duration", durationInSeconds);
    }

    if (expirationUnix != 0)
    {
      stickyMessageDict.Add("expiration", expirationUnix);
    }

    if (visibleFromUnix != 0)
    {
      stickyMessageDict.Add("visibleFrom", visibleFromUnix);
    }

    if (language != null)
    {
      stickyMessageDict.Add("language", language);
    }
    
    GenericData stickyMessage = GenericData.FromDictionary(stickyMessageDict);
  
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/messages/sticky"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"message", stickyMessage}
                  })
      .OnSuccess((sender, apiResponse) =>
                 {
                   SetStatus("Successfully created chat announcement.", RequestStatus.Success);
                   Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                 })
      .OnFailure((sender, apiResponse) =>
                 {
                   SetStatus("Failed to create chat announcement.", RequestStatus.Error);
                   Log.Error(owner: Owner.Nathan, message: "Request to chat-service failed.", data: new
                                                                                                    {
                                                                                                      Response = apiResponse
                                                                                                    });
                 })
      .Post(out GenericData response, out int code);

    return RedirectToAction("Announcements");
  }
  
  [HttpPost]
  [Route("announcements/expire")]
  public async Task<IActionResult> AnnouncementsExpire(string messageId)
  {
    // Checking access permissions
    if (!Permissions.Chat.View_Page || !Permissions.Chat.Edit)
    {
      return View("Error");
    }
    
    ClearStatus();
  
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/messages/unsticky"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"messageId", messageId}
                  })
      .OnSuccess((sender, apiResponse) =>
                 {
                   SetStatus("Successfully expired chat announcement.", RequestStatus.Success);
                   Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                 })
      .OnFailure((sender, apiResponse) =>
                 {
                   SetStatus("Failed to expire chat announcement.", RequestStatus.Error);
                   Log.Error(owner: Owner.Nathan, message: "Request to chat-service failed.", data: new
                                                                                                    {
                                                                                                      Response = apiResponse
                                                                                                    });
                 })
      .Post(out GenericData response, out int code);

    return RedirectToAction("Announcements");
  }
  
  [Route("reports")]
  public async Task<IActionResult> Reports()
  {
    // Checking access permissions
    if (!Permissions.Chat.View_Page)
    {
      return View("Error");
    }
    
    ClearStatus();
  
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/reports/list"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .OnSuccess((sender, apiResponse) =>
                 {
                   SetStatus("Successfully fetched chat reports.", RequestStatus.Success);
                   Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                 })
      .OnFailure((sender, apiResponse) =>
                 {
                   SetStatus("Failed to fetch chat reports.", RequestStatus.Error);
                   Log.Error(owner: Owner.Nathan, message: "Request to chat-service failed.", data: new
                                                                                              {
                                                                                                Response = apiResponse
                                                                                              });
                 })
      .Get(out GenericData response, out int code);

    List<ChatReport> chatReportsList = new List<ChatReport>();

    try
    {
      chatReportsList = response.Require<List<ChatReport>>(key: "reports");
    }
    catch (Exception e)
    {
      Log.Error(owner: Owner.Nathan, message: "Failed to parse response from chat-service.", data: e);
    }
    
    ViewData["ChatReports"] = chatReportsList;
  
    return View();
  }

  [Route("player")]
  public async Task<IActionResult> Player()
  {
    // Checking access permissions
    if (!Permissions.Chat.View_Page)
    {
      return View("Error");
    }
    
    return View();
  }

  [Route("player/{accountId}")]
  public async Task<IActionResult> Player(string accountId)
  {
    // Checking access permissions
    if (!Permissions.Chat.View_Page)
    {
      return View("Error");
    }

    ClearStatus();

    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/playerDetails"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"aid", accountId}
                  })
      .OnSuccess((sender, apiResponse) =>
                 {
                   SetStatus("Successfully fetched chat player details.", RequestStatus.Success);
                   Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                 })
      .OnFailure((sender, apiResponse) =>
                 {
                   SetStatus("Failed to fetch chat player details.", RequestStatus.Error);
                   Log.Error(owner: Owner.Nathan, message: "Request to chat-service failed.", data: new
                                                                                                    {
                                                                                                      Response =
                                                                                                        apiResponse
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
  [Route("playerSearch")]
  public async Task<IActionResult> PlayerSearch(string accountId)
  {
    // Checking access permissions
    if (!Permissions.Chat.View_Page)
    {
      return View("Error");
    }

    return RedirectToAction("Player", new { accountId = accountId });
  }

  [HttpPost]
  [Route("ban")]
  public async Task<IActionResult> Ban(string accountId, string reason, long? duration)
  {
    // Checking access permissions
    if (!Permissions.Chat.View_Page || !Permissions.Chat.Edit)
    {
      return View("Error");
    }
    
    ClearStatus();
        
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/ban/player"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"aid", accountId},
                    {"reason", reason},
                    {"durationInSeconds", duration}
                  })
      .OnSuccess(((sender, apiResponse) =>
                  {
                    SetStatus("Successfully banned player.", RequestStatus.Success);
                    Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                  }))
      .OnFailure(((sender, apiResponse) =>
                  {
                    SetStatus("Failed to ban player.", RequestStatus.Error);
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
  public async Task<IActionResult> Unban(string accountId)
  {
    // Checking access permissions
    if (!Permissions.Chat.View_Page || !Permissions.Chat.Edit)
    {
      return View("Error");
    }
    
    ClearStatus();
        
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/ban/lift"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"accountId", accountId}
                  })
      .OnSuccess(((sender, apiResponse) =>
                  {
                    SetStatus("Successfully unbanned player.", RequestStatus.Success);
                    Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                  }))
      .OnFailure(((sender, apiResponse) =>
                  {
                    SetStatus("Failed to unban player.", RequestStatus.Error);
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
    if (!Permissions.Chat.View_Page || !Permissions.Chat.Edit)
    {
      return View("Error");
    }
    
    ClearStatus();
        
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/reports/ignore"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"reportId", reportId}
                  })
      .OnSuccess(((sender, apiResponse) =>
                  {
                    SetStatus("Successfully ignored report.", RequestStatus.Success);
                    Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                  }))
      .OnFailure(((sender, apiResponse) =>
                  {
                    SetStatus("Failed to ignore report.", RequestStatus.Error);
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
    if (!Permissions.Chat.View_Page || !Permissions.Chat.Edit)
    {
      return View("Error");
    }
    
    ClearStatus();
        
    _apiService
      .Request(PlatformEnvironment.Url("/chat/admin/reports/delete"))
      .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("chatToken"))
      .SetPayload(new GenericData
                  {
                    {"reportId", reportId}
                  })
      .OnSuccess(((sender, apiResponse) =>
                  {
                    SetStatus("Successfully deleted report.", RequestStatus.Success);
                    Log.Local(Owner.Nathan, "Request to chat-service succeeded.");
                  }))
      .OnFailure(((sender, apiResponse) =>
                  {
                    SetStatus("Failed to delete report.", RequestStatus.Error);
                    Log.Error(Owner.Nathan, "Request to chat-service failed.", data: new
                                                                                       {
                                                                                         Response = apiResponse
                                                                                       });
                  }))
      .Post(out GenericData response, out int code);
  
    return RedirectToAction("Player", new { accountId = accountId });
  }
}