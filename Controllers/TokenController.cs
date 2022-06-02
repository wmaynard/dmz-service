using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;
using TowerPortal.Services;
using TowerPortal.Utilities;

namespace TowerPortal.Controllers;

[Authorize]
[Route("token")]
public class TokenController : PlatformController
{
    private readonly ApiService _apiService;
    private readonly DynamicConfigService _dynamicConfigService;
    private readonly AccountService _accountService;
    private readonly TokenLogService _tokenLogService;

    [Route("ban")]
    public async Task<IActionResult> Ban()
    {
        // Checking access permissions
        Account account = Account.FromGoogleClaims(User.Claims);
        Account mongoAccount = _accountService.FindOne(mongo => mongo.Email == account.Email);
        ViewData["Permissions"] = mongoAccount.Permissions;
        Permissions currentPermissions = _accountService.CheckPermissions(mongoAccount);
        // Tab view permissions
        bool currentAdmin = currentPermissions.Admin;
        bool currentManagePermissions = currentPermissions.ManagePermissions;
        bool currentViewPlayer = currentPermissions.ViewPlayer;
        bool currentViewMailbox = currentPermissions.ViewMailbox;
        bool currentViewToken = currentPermissions.ViewToken;
        bool currentEditToken = currentPermissions.EditToken;
        if (currentAdmin)
        {
            ViewData["CurrentAdmin"] = currentPermissions.Admin;
        }
        if (currentManagePermissions)
        {
            ViewData["CurrentManagePermissions"] = currentPermissions.ManagePermissions;
        }
        if (currentViewPlayer)
        {
            ViewData["CurrentViewPlayer"] = currentPermissions.ViewPlayer;
        }
        if (currentViewMailbox)
        {
            ViewData["CurrentViewMailbox"] = currentPermissions.ViewMailbox;
        }
        if (currentViewToken)
        {
            ViewData["CurrentViewToken"] = currentPermissions.ViewToken;
        }
        if (currentEditToken)
        {
            ViewData["CurrentEditToken"] = currentPermissions.EditToken;
        }
        
        // Redirect if not allowed
        if (currentViewToken == false)
        {
            return View("Error");
        }

        try
        {
            List<TokenLog> tokenLogs = _tokenLogService.GetLogs();
            ViewData["TokenLogs"] = tokenLogs;
            
            TempData["Success"] = "";
            TempData["Failure"] = null;
        }
        catch (Exception e)
        {
            Log.Error(owner: Owner.Nathan, message: "Failed to fetch token logs.", data: e.Message);

            TempData["Success"] = "Failed to fetch logs.";
            TempData["Failure"] = true;
        }
        
        return View();
    }
    
    // post for main

    [HttpPost]
    [Route("ban")]
    public async Task<IActionResult> Ban(string playerIds, string action, string unbanTime, string note)
    {
        // Checking access permissions
        Account account = Account.FromGoogleClaims(User.Claims);
        Account mongoAccount = _accountService.FindOne(mongo => mongo.Email == account.Email);
        ViewData["Permissions"] = mongoAccount.Permissions;
        Permissions currentPermissions = _accountService.CheckPermissions(mongoAccount);
        // Tab view permissions
        bool currentAdmin = currentPermissions.Admin;
        bool currentManagePermissions = currentPermissions.ManagePermissions;
        bool currentViewPlayer = currentPermissions.ViewPlayer;
        bool currentViewMailbox = currentPermissions.ViewMailbox;
        bool currentViewToken = currentPermissions.ViewToken;
        bool currentEditToken = currentPermissions.EditToken;
        if (currentAdmin)
        {
            ViewData["CurrentAdmin"] = currentPermissions.Admin;
        }
        if (currentManagePermissions)
        {
            ViewData["CurrentManagePermissions"] = currentPermissions.ManagePermissions;
        }
        if (currentViewPlayer)
        {
            ViewData["CurrentViewPlayer"] = currentPermissions.ViewPlayer;
        }
        if (currentViewMailbox)
        {
            ViewData["CurrentViewMailbox"] = currentPermissions.ViewMailbox;
        }
        if (currentViewToken)
        {
            ViewData["CurrentViewToken"] = currentPermissions.ViewToken;
        }
        if (currentEditToken)
        {
            ViewData["CurrentEditToken"] = currentPermissions.EditToken;
        }
        
        // Redirect if not allowed
        if (currentViewToken == false || currentEditToken == false)
        {
            return View("Error");
        }
        
        string token = _dynamicConfigService.GameConfig.Require<string>("portalToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/token/admin/";
        //string requestUrl = PlatformEnvironment.Url("/token/admin/");

        if (action == "ban")
        {
            try
            {
                List<string> playerIdsList = ParseMessageData.ParseIds(playerIds);
                long unbanTimeUnix = ParseMessageData.ParseDateTime(unbanTime);
                string actor = mongoAccount.Name;

                foreach (string playerId in playerIdsList)
                {
                    _apiService
                        .Request(requestUrl + "ban")
                        .AddAuthorization(token)
                        .SetPayload(new GenericData
                        {
                            {"aid", playerId}
                            // TODO send duration to service when supported
                        })
                        .OnSuccess((sender, apiResponse) =>
                        {
                            TempData["Success"] = "Successfully banned player(s).";
                            TempData["Failure"] = null;
                            Log.Local(owner: Owner.Nathan, message: "Request to token-service succeeded.");
                        })
                        .OnFailure((sender, apiResponse) =>
                        {
                            TempData["Success"] = "Failed to ban player(s).";
                            TempData["Failure"] = true;
                            Log.Error(owner: Owner.Nathan, message: "Request to token-service failed.", data: new
                            {
                                Response = apiResponse
                            });
                        })
                        .Patch(out GenericData sendResponse, out int sendCode);

                    TokenLog log = null; // TODO move logs to separate page, only admins
                    if (unbanTimeUnix == 0)
                    {
                        log = new TokenLog(actor: actor, action: "ban", unbanTime: null, target: playerId,
                            note: note);
                    }
                    else
                    {
                        log = new TokenLog(actor: actor, action: "ban", unbanTime: unbanTimeUnix, target: playerId,
                            note: note);
                    }
                    _tokenLogService.Create(log);
                }
            }
            catch (Exception e)
            {
                TempData["Success"] = "Failed to ban player(s). Some fields may be malformed.";
                TempData["Failure"] = true;
                Log.Error(owner: Owner.Nathan, message: "Error occurred when banning player(s).", data: e.Message);
            }
        }

        if (action == "unban")
        {
            try
            {
                List<string> playerIdsList = ParseMessageData.ParseIds(playerIds);
                string actor = mongoAccount.Name;

                foreach (string playerId in playerIdsList)
                {
                    _apiService
                        .Request(requestUrl + "unban")
                        .AddAuthorization(token)
                        .SetPayload(new GenericData
                        {
                            {"aid", playerId}
                        })
                        .OnSuccess((sender, apiResponse) =>
                        {
                            TempData["Success"] = "Successfully unbanned player(s).";
                            TempData["Failure"] = null;
                            Log.Local(owner: Owner.Nathan, message: "Request to token-service succeeded.");
                        })
                        .OnFailure((sender, apiResponse) =>
                        {
                            TempData["Success"] = "Failed to unban player(s).";
                            TempData["Failure"] = true;
                            Log.Error(owner: Owner.Nathan, message: "Request to token-service failed.", data: new
                            {
                                Response = apiResponse
                            });
                        })
                        .Patch(out GenericData sendResponse, out int sendCode);

                    TokenLog log = new TokenLog(actor: actor, action: "unban", unbanTime: null, target: playerId,
                        note: note);
                    _tokenLogService.Create(log);
                }
            }
            catch (Exception e)
            {
                TempData["Success"] = "Failed to unban player(s). Some fields may be malformed.";
                TempData["Failure"] = true;
                Log.Error(owner: Owner.Nathan, message: "Error occurred when unbanning player(s).", data: e.Message);
            }
        }

        if (action == "logout")
        {
            try
            {
                List<string> playerIdsList = ParseMessageData.ParseIds(playerIds);
                string actor = mongoAccount.Name;

                foreach (string playerId in playerIdsList)
                {
                    _apiService
                        .Request(requestUrl + "invalidate")
                        .AddAuthorization(token)
                        .SetPayload(new GenericData
                        {
                            {"aid", playerId}
                        })
                        .OnSuccess((sender, apiResponse) =>
                        {
                            TempData["Success"] = "Successfully invalidated token for player(s).";
                            TempData["Failure"] = null;
                            Log.Local(owner: Owner.Nathan, message: "Request to token-service succeeded.");
                        })
                        .OnFailure((sender, apiResponse) =>
                        {
                            TempData["Success"] = "Failed to invalidate token for player(s).";
                            TempData["Failure"] = true;
                            Log.Error(owner: Owner.Nathan, message: "Request to token-service failed.", data: new
                            {
                                Response = apiResponse
                            });
                        })
                        .Patch(out GenericData sendResponse, out int sendCode);

                    TokenLog log = new TokenLog(actor: actor, action: "invalidate", unbanTime: null, target: playerId,
                        note: note);
                    _tokenLogService.Create(log);
                }
            }
            catch (Exception e)
            {
                TempData["Success"] = "Failed to invalidate token for player(s). Some fields may be malformed.";
                TempData["Failure"] = true;
                Log.Error(owner: Owner.Nathan, message: "Error occurred when invalidating token for player(s).", data: e.Message);
            }
        }
        
        List<TokenLog> tokenLogs = _tokenLogService.GetLogs();
        ViewData["TokenLogs"] = tokenLogs;
            
        return View();
    }
}