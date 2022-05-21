using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Authorize]
[Route("player")]
public class PlayerController : PlatformController
{
#pragma warning disable CS0649
    private readonly ApiService _apiService;
    private readonly DynamicConfigService _dynamicConfigService;
    private readonly AccountService _accountService;
#pragma warning restore CS0649
    
    [Route("search")]
    public async Task<IActionResult> Search(string query)
    {
        ViewData["Message"] = "Player search";
        
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
        
        // Redirect if not allowed
        if (currentViewPlayer == false)
        {
            return View("Error");
        }

        if (query != null)
        {
            List<string> searchId = new List<string>();
            List<string> searchUser = new List<string>();

            string token = _dynamicConfigService.GameConfig.Require<string>("playerServiceToken");
            string requestUrl = PlatformEnvironment.Url($"/player/v2/admin/search?term={query}");
            
            // Use the API Service to simplify web requests
            _apiService
                .Request(requestUrl)
                .AddAuthorization(token)
                .OnSuccess(((sender, apiResponse) =>
                {
                    Log.Local(Owner.Nathan, "Request to player-service-v2 succeeded.");
                }))
                .OnFailure(((sender, apiResponse) =>
                {
                    Log.Error(Owner.Nathan, "Request to player-service-v2 failed.", data: new
                    {
                        Url = requestUrl,
                        Response = apiResponse
                    });
                }))
                .Get(out GenericData response, out int code);
            
            string responseString = response.JSON;

            SearchResponse searchResponse = JsonConvert.DeserializeObject<SearchResponse>(responseString);

            foreach (SearchResult result in searchResponse.Results)
            {
                searchId.Add(result.Player.Id);
                searchUser.Add(result.Player.Username);
            }
            
            List<List<string>> searchList = new List<List<string>>();
            for (int i = 0; i < searchId.Count; i++)
            {
                List<string> searchEntry = new List<string>();
                searchEntry.Add(searchId[i]);
                searchEntry.Add(searchUser[i]);
                
                searchList.Add(searchEntry);
            }

            ViewData["Query"] = query;
            ViewData["Data"] = searchList;
            
            return View();
        }
        ViewData["Data"] = new List<List<string>>();
        return View();
    }
    
    [Route("details")]
    public async Task<IActionResult> Details(string id)
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
        
        // Redirect if not allowed
        if (currentViewPlayer == false)
        {
            return View("Error");
        }
        
        string requestUrl = PlatformEnvironment.Url($"/player/v2/admin/details?accountId={id}");
        string token = _dynamicConfigService.GameConfig.Require<string>("playerServiceToken");
        
        Log.Local(owner: Owner.Nathan, "Admin portal request URL:", data: requestUrl);
        
        _apiService
            .Request(requestUrl)
            .AddAuthorization(token)
            .OnSuccess(((sender, apiResponse) =>
            {
                Log.Local(Owner.Nathan, "Request to player-service-v2 details succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                Log.Error(Owner.Nathan, "Request to player-service-v2 details failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Get(out GenericData response, out int code);

        ViewData["accountId"] = id;

        string responseString = response.JSON;
        
        DetailsResponse detailsResponse = JsonConvert.DeserializeObject<DetailsResponse>(responseString);

        ViewData["ClientVersion"] = detailsResponse.Player.ClientVersion;
        ViewData["DateCreated"] = detailsResponse.Player.DateCreated;
        ViewData["DataVersion"] = detailsResponse.Player.DataVersion;
        ViewData["DeviceType"] = detailsResponse.Player.DeviceType;
        ViewData["LastSavedInstallId"] = detailsResponse.Player.LastSavedInstallId;
        ViewData["MergeVersion"] = detailsResponse.Player.MergeVersion;
        ViewData["LastChanged"] = detailsResponse.Player.LastChanged;
        ViewData["LastDataVersion"] = detailsResponse.Player.LastDataVersion;
        ViewData["Screenname"] = detailsResponse.Player.Screenname;
        ViewData["LastUpdated"] = detailsResponse.Player.LastUpdated;
        ViewData["Discriminator"] = detailsResponse.Player.Discriminator;
        ViewData["Username"] = detailsResponse.Player.Username;
        ViewData["Id"] = detailsResponse.Player.Id;
        
        ViewData["Profiles"] = detailsResponse.Profiles;
        ViewData["Items"] = detailsResponse.Items;

        return View();
    }

    [HttpPost]
    [Route("editScreenname")]
    public async Task<IActionResult> EditScreenname(string accountId, string editScreenname)
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
        bool currentEditPlayer = currentPermissions.EditPlayer;
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
        if (currentEditPlayer)
        {
            ViewData["CurrentEditPlayer"] = currentPermissions.EditPlayer;
        }
        
        // Redirect if not allowed
        if (currentEditPlayer == false)
        {
            return View("Error");
        }
        
        string token = _dynamicConfigService.GameConfig.Require<string>("playerServiceToken");
        string requestUrl = PlatformEnvironment.Url("/player/v2/admin/screenname");
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        _apiService
            .Request(requestUrl)
            .AddAuthorization(token)
            .SetPayload(new GenericData
            {
                {"accountId", accountId},
                {"screenname", editScreenname}
            })
            .OnSuccess(((sender, apiResponse) =>
            {
                TempData["Success"] = "Successfully edited player screenname.";
                TempData["Failure"] = null;
                Log.Local(Owner.Nathan, "Request to player-service-v2 screenname succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                TempData["Success"] = "Failed to edit player screenname.";
                TempData["Failure"] = true;
                Log.Error(Owner.Nathan, "Request to player-service-v2 screenname failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Patch(out GenericData response, out int code);

        if (response == null)
        {
            TempData["Success"] = "Response was null.";
            TempData["Failure"] = true;

            return RedirectToAction("Details", new { id = accountId });
        }
        
        return RedirectToAction("Details", new { id = accountId });
    }

    [Route("health")]
    public override ActionResult HealthCheck() => Ok(_apiService.HealthCheckResponseObject, _dynamicConfigService.HealthCheckResponseObject);
}