using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RCL.Logging;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Enums;
using TowerPortal.Models.Player;

namespace TowerPortal.Controllers;

[Authorize]
[Route("portal/player")]
public class PlayerController : PortalController
{
#pragma warning disable CS0649
    private readonly DynamicConfigService _dynamicConfigService;
#pragma warning restore CS0649
    
    #region Player search
    
    // Search for player
    [Route("search")]
    public async Task<IActionResult> Search(string query)
    {
        ViewData["Message"] = "Player search";

        // Exit early if there's invalid data; easier to read.
        if (!Permissions.Player.View_Page)
        {
            return View("AccessDenied");
        }

        if (query == null)
        {
            ViewData["Data"] = new List<List<string>>();
            return View();
        }

        // Use the API Service to simplify web requests
        _apiService
            .Request(PlatformEnvironment.Url("/player/v2/admin/search"))
            .AddParameter("term", query)
            .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("playerServiceToken"))
            .OnSuccess((_, apiResponse) =>
            {
                Log.Local(Owner.Nathan, "Request to player-service-v2 succeeded.");
            })
            .OnFailure((_, apiResponse) =>
            {
                Log.Error(Owner.Nathan, "Request to player-service-v2 failed.", data: new
                {
                    Url = apiResponse.RequestUrl,
                    Response = apiResponse
                });
            })
            .Get(out GenericData response);

        // Will: Probably? don't need the SearchResult model.  I think the game server serializes the entire response from
        // the APIs it touches, but general practice for platform is just to serialize the keys you need.
        SearchResult[] results = response.Require<SearchResult[]>(SearchResult.API_KEY_RESULTS);
        
        // This could probably be converted to a struct or something; nested collections are difficult to work with.
        // LINQ can also help a lot with transforming data from one structure into another, and is easier to read (with practice)
        // than looping through collections.
        List<List<string>> searchList = results
            .Select(result => new List<string> { result.Player.Id, result.Player.Username })
            .ToList();

        ViewData["Query"] = query;
        ViewData["Data"] = searchList;
        
        return View();
    }
    
    #endregion
    
    #region Player details
    
    // Displays player details
    [Route("details")]
    public async Task<IActionResult> Details(string id)
    {
        // Checking access permissions
        if (!Permissions.Player.View_Page)
        {
            return View("AccessDenied");
        }
        
        ClearStatus();

        _apiService
            .Request(PlatformEnvironment.Url("/player/v2/admin/details"))
            .AddParameter("accountId", id)
            .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("playerServiceToken"))
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

        DetailsResponse detailsResponse = new DetailsResponse();
        // TODO remove newtonsoft, breaking down into models

        PlayerComponents component = new PlayerComponents();

        try
        {
            component = response.Require<PlayerComponents>(key: "components");
            detailsResponse = JsonConvert.DeserializeObject<DetailsResponse>(responseString);
            SetStatus("Successfully fetched player details.", RequestStatus.Success);
        }
        catch (Exception e)
        {
            SetStatus("Failed to fetch player details.", RequestStatus.Error);
            Log.Error(owner: Owner.Nathan, message: "Failed to parse response from player-service.", data: e);
        }

        try
        {
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
            ViewData["Components"] = component;
            ViewData["Items"] = detailsResponse.Items;

            PlayerWallet playerWallet = component.Wallet;
            ViewData["WalletCurrencies"] = playerWallet.Data.Currencies;
            
            SetStatus("Successfully fetched player details.", RequestStatus.Success);
        }
        catch (Exception e)
        {
            SetStatus("Failed to fetch player details. Player could not be found.", RequestStatus.Error);
            Log.Error(owner: Owner.Nathan, message: "Player details request failed.", data:"Details response was missing data. It is possible that a player that does not exist was requested.", exception: e);
            return View("Error");
        }

        return View();
    }

    // Sends request to edit player screenname
    [HttpPost]
    [Route("editScreenname")]
    public async Task<IActionResult> EditScreenname(string accountId, string editScreenname)
    {
        // Checking access permissions
        if (!Permissions.Player.View_Page || !Permissions.Player.Edit)
        {
            return View("AccessDenied");
        }

        ClearStatus();
        
        _apiService
            .Request(PlatformEnvironment.Url("/player/v2/admin/screenname"))
            .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("playerServiceToken"))
            .SetPayload(new GenericData
            {
                {"accountId", accountId},
                {"screenname", editScreenname}
            })
            .OnSuccess(((sender, apiResponse) =>
            {
                SetStatus("Successfully edited player screenname.", RequestStatus.Success);
                Log.Local(Owner.Nathan, "Request to player-service-v2 screenname succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                SetStatus("Failed to edit player screenname.", RequestStatus.Error);
                Log.Error(Owner.Nathan, "Request to player-service-v2 screenname failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Patch(out GenericData response, out int code);

        return RedirectToAction("Details", new { id = accountId });
    }
    
    // Sends request to modify player wallet quantities
    [HttpPost]
    [Route("EditWallet")]
    public async Task<IActionResult> EditWallet(IFormCollection collection)
    {
        // Checking access permissions
        if (!Permissions.Player.View_Page || !Permissions.Player.Edit)
        {
            return View("AccessDenied");
        }

        string aid = collection["aid"];
        
        // PlayerComponents component = (PlayerComponents) TempData["Components"]; 
        // TODO resolve to remove following extra call; this is unable to pass data for some reason
        
        _apiService
            .Request(PlatformEnvironment.Url("/player/v2/admin/details"))
            .AddParameter("accountId", aid)
            .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("playerServiceToken"))
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
            .Get(out GenericData tempResponse, out int tempCode);
        
        PlayerComponents component = new PlayerComponents();

        try
        {
            component = tempResponse.Require<PlayerComponents>(key: "components");
        }
        catch (Exception e)
        {
            SetStatus("Failed to update player wallet.", RequestStatus.Error);
            Log.Error(owner: Owner.Nathan, message: "Failed to parse response from player-service.", data: e);
        }

        if (component == null)
        {
            SetStatus("Failed to update player wallet.", RequestStatus.Error);
            Log.Error(owner: Owner.Nathan, message: "Error occurred when attempting to update player components.", data:"TempData components was null when passed from player details to player wallets.");
            return RedirectToAction("Details", new { id = aid});
        }

        List<WalletCurrency> newWalletCurrencies = new List<WalletCurrency>();

        foreach (string key in collection.Keys)
        {
            if (key != "aid" && key != "__RequestVerificationToken")
            {
                newWalletCurrencies.Add(new WalletCurrency(currencyId: key, amount: int.Parse(collection[key])));
            }
        }

        component.Wallet.Data.Currencies = newWalletCurrencies;
        component.Wallet.Version += 1;

        ClearStatus();
        
        _apiService
            .Request(PlatformEnvironment.Url("/player/v2/admin/component"))
            .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("playerServiceToken"))
            .SetPayload(new GenericData
            {
                {"component", component.Wallet}
            })
            .OnSuccess(((sender, apiResponse) =>
            {
                SetStatus("Successfully edited player wallet.", RequestStatus.Success);
                Log.Local(Owner.Nathan, "Request to player-service-v2 update succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                SetStatus("Failed to update player wallet.", RequestStatus.Error);
                Log.Error(Owner.Nathan, "Request to player-service-v2 update failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Patch(out GenericData response, out int code);

        return RedirectToAction("Details", new { id = aid });
    }
    
    #endregion
}