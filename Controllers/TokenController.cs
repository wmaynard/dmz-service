using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Enums;
using TowerPortal.Models.Portal;
using TowerPortal.Models.Token;
using TowerPortal.Services;
using TowerPortal.Utilities;

namespace TowerPortal.Controllers;

[Authorize]
[Route("portal/token")]
public class TokenController : PortalController
{
#pragma warning disable CS0649
    private readonly DynamicConfigService _dynamicConfigService;
    private readonly TokenLogService _tokenLogService;
#pragma warning restore CS0649

    // Loads page
    [Route("ban")]
    public async Task<IActionResult> Ban()
    {
        // Checking access permissions
        if (!Permissions.Token.View_Page)
        {
            return View("AccessDenied");
        }

        try
        {
            List<TokenLog> tokenLogs = _tokenLogService.GetLogs();
            ViewData["TokenLogs"] = tokenLogs;
            
            ClearStatus();
        }
        catch (Exception e)
        {
            Log.Error(owner: Owner.Nathan, message: "Failed to fetch token logs.", data: e.Message);

            SetStatus("Failed to fetch logs.", RequestStatus.Error);
        }
        
        return View();
    }
    
    // Request to ban/unban/logout player(s)
    [HttpPost]
    [Route("ban")]
    public async Task<IActionResult> Ban(string playerIds, string action, string unbanTime, string note)
    {
        // Checking access permissions
        if (!Permissions.Token.View_Page || !Permissions.Token.Edit)
        {
            return View("AccessDenied");
        }

        if (action == "ban")
        {
            if (unbanTime == null)
            {
                try
                {
                    List<string> playerIdsList = ParseMessageData.ParseIds(playerIds);
                    string actor = User?.Identity?.Name;

                    foreach (string playerId in playerIdsList)
                    {
                        _apiService
                            .Request(PlatformEnvironment.Url("/token/admin/ban"))
                            .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("portalToken"))
                            .SetPayload(new GenericData
                            {
                                {"aid", playerId}
                            })
                            .OnSuccess((sender, apiResponse) =>
                            {
                                SetStatus("Successfully banned players(s).", RequestStatus.Success);
                                Log.Local(owner: Owner.Nathan, message: "Request to token-service succeeded.");
                                
                                TokenLog log = null; // TODO move logs to separate page, only admins
                                log = new TokenLog(actor: actor, action: "ban", unbanTime: null, target: playerId,
                                    note: note);
                                _tokenLogService.Create(log);
                            })
                            .OnFailure((sender, apiResponse) =>
                            {
                                SetStatus("Failed to ban player(s).", RequestStatus.Error);
                                Log.Error(owner: Owner.Nathan, message: "Request to token-service failed.", data: new
                                {
                                    Response = apiResponse
                                });
                            })
                            .Patch(out GenericData sendResponse, out int sendCode);
                    }
                }
                catch (Exception e)
                {
                    SetStatus("Failed to ban player(s). Some fields may be malformed.", RequestStatus.Error);
                    Log.Error(owner: Owner.Nathan, message: "Error occurred when banning player(s).", data: e.Message);
                }
            }
            else
            {  
                try
                {
                    List<string> playerIdsList = ParseMessageData.ParseIds(playerIds);
                    long unbanTimeUnix = ParseMessageData.ParseDateTime(unbanTime + "T00:00");
                    long duration = unbanTimeUnix - Account.UnixTime;
                    string actor = User?.Identity?.Name;

                    foreach (string playerId in playerIdsList)
                    {
                        _apiService
                            .Request(PlatformEnvironment.Url("/token/admin/ban"))
                            .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("portalToken"))
                            .SetPayload(new GenericData
                            {
                                {"aid", playerId},
                                {"duration", duration}
                            })
                            .OnSuccess((sender, apiResponse) =>
                            {
                                SetStatus("Successfully banned player(s).", RequestStatus.Success);
                                Log.Local(owner: Owner.Nathan, message: "Request to token-service succeeded.");
                                
                                TokenLog log = null; // TODO move logs to separate page, only admins
                                log = new TokenLog(actor: actor, action: "ban", unbanTime: unbanTimeUnix, target: playerId,
                                    note: note);
                                _tokenLogService.Create(log);
                            })
                            .OnFailure((sender, apiResponse) =>
                            {
                                SetStatus("Failed to ban player(s).", RequestStatus.Error);
                                Log.Error(owner: Owner.Nathan, message: "Request to token-service failed.", data: new
                                {
                                    Response = apiResponse
                                });
                            })
                            .Patch(out GenericData sendResponse, out int sendCode);
                    }
                }
                catch (Exception e)
                {
                    SetStatus("Failed to ban player(s). Some fields may be malformed.", RequestStatus.Error);
                    Log.Error(owner: Owner.Nathan, message: "Error occurred when banning player(s).", data: e.Message);
                }
            }
        }

        if (action == "unban")
        {
            try
            {
                List<string> playerIdsList = ParseMessageData.ParseIds(playerIds);
                string actor = User?.Identity?.Name;

                foreach (string playerId in playerIdsList)
                {
                    _apiService
                        .Request(PlatformEnvironment.Url("/token/admin/unban"))
                        .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("portalToken"))
                        .SetPayload(new GenericData
                        {
                            {"aid", playerId}
                        })
                        .OnSuccess((sender, apiResponse) =>
                        {
                            SetStatus("Successfully unbanned player(s).", RequestStatus.Success);
                            Log.Local(owner: Owner.Nathan, message: "Request to token-service succeeded.");
                            
                            TokenLog log = new TokenLog(actor: actor, action: "unban", unbanTime: null, target: playerId,
                                note: note);
                            _tokenLogService.Create(log);
                        })
                        .OnFailure((sender, apiResponse) =>
                        {
                            SetStatus("Failed to unban player(s).", RequestStatus.Error);
                            Log.Error(owner: Owner.Nathan, message: "Request to token-service failed.", data: new
                            {
                                Response = apiResponse
                            });
                        })
                        .Patch(out GenericData sendResponse, out int sendCode);
                }
            }
            catch (Exception e)
            {
                SetStatus("Failed to unban player(s). Some fields may be malformed.", RequestStatus.Error);
                Log.Error(owner: Owner.Nathan, message: "Error occurred when unbanning player(s).", data: e.Message);
            }
        }

        if (action == "logout")
        {
            try
            {
                List<string> playerIdsList = ParseMessageData.ParseIds(playerIds);
                string actor = User?.Identity?.Name;

                foreach (string playerId in playerIdsList)
                {
                    _apiService
                        .Request(PlatformEnvironment.Url("/token/admin/invalidate"))
                        .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("portalToken"))
                        .SetPayload(new GenericData
                        {
                            {"aid", playerId}
                        })
                        .OnSuccess((sender, apiResponse) =>
                        {
                            SetStatus("Successfully invalidated token for player(s).", RequestStatus.Success);
                            Log.Local(owner: Owner.Nathan, message: "Request to token-service succeeded.");
                            
                            TokenLog log = new TokenLog(actor: actor, action: "invalidate", unbanTime: null, target: playerId,
                                note: note);
                            _tokenLogService.Create(log);
                        })
                        .OnFailure((sender, apiResponse) =>
                        {
                            SetStatus("Failed to invalidate token for player(s).", RequestStatus.Error);
                            Log.Error(owner: Owner.Nathan, message: "Request to token-service failed.", data: new
                            {
                                Response = apiResponse
                            });
                        })
                        .Patch(out GenericData sendResponse, out int sendCode);
                }
            }
            catch (Exception e)
            {
                SetStatus("Failed to invalidate token for player(s). Some fields may be malformed.", RequestStatus.Error);
                Log.Error(owner: Owner.Nathan, message: "Error occurred when invalidating token for player(s).", data: e.Message);
            }
        }
        
        List<TokenLog> tokenLogs = _tokenLogService.GetLogs();
        ViewData["TokenLogs"] = tokenLogs;
            
        return View();
    }
}