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
using Rumble.Platform.Common.Web;
using TowerPortal.Models;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Authorize]
[Route("portal/player")]
public class PlayerController : PortalController
{
#pragma warning disable CS0649
    private readonly DynamicConfigService _dynamicConfigService;
    private readonly AccountService _accountService;
#pragma warning restore CS0649
    
    [Route("search")]
    public async Task<IActionResult> Search(string query)
    {
        ViewData["Message"] = "Player search";

        // Exit early if there's invalid data; easier to read.
        if (!UserPermissions.ViewPlayer)
            return View("Error");
        if (query == null)
        {
            ViewData["Data"] = new List<List<string>>();
            return View();
        }

        List<string> searchId = new List<string>();
        List<string> searchUser = new List<string>();

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
        bool currentViewToken = currentPermissions.ViewToken;
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
        
        // Redirect if not allowed
        if (currentViewPlayer == false)
        {
            return View("Error");
        }

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
        
        DetailsResponse detailsResponse = JsonConvert.DeserializeObject<DetailsResponse>(responseString);
        // TODO remove newtonsoft, breaking down into models
        
        
        PlayerComponents component = response.Require<PlayerComponents>(key: "components");
        
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

        PlayerComponents playerComponents = response.Require<PlayerComponents>(key: "components");
        PlayerWallet playerWallet = playerComponents.Wallet; // TODO simplify if GenericData works
        ViewData["WalletCurrencies"] = playerWallet.Data.Currencies;

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
        bool currentViewToken = currentPermissions.ViewToken;
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
        if (currentViewToken)
        {
            ViewData["CurrentViewToken"] = currentPermissions.ViewToken;
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
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
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

        return RedirectToAction("Details", new { id = accountId });
    }
    
    [HttpPost]
    [Route("EditWallet")]
    public async Task<IActionResult> EditWallet(IFormCollection collection)
    // hard coded in currencies, possibly subject to changes
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
        if (currentViewToken)
        {
            ViewData["CurrentViewToken"] = currentPermissions.ViewToken;
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
        
        PlayerComponents component = tempResponse.Require<PlayerComponents>(key: "components");

        if (component == null)
        {
            Log.Error(owner: Owner.Nathan, message: "Error occurred when attempting to update player components.", data:"TempData components was null when passed from player details to player wallets.");
            TempData["Success"] = "Failed to update player wallet.";
            TempData["Failure"] = true;
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

        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        _apiService
            .Request(PlatformEnvironment.Url("/player/v2/admin/component"))
            .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("playerServiceToken"))
            .SetPayload(new GenericData
            {
                {"component", component.Wallet}
            })
            .OnSuccess(((sender, apiResponse) =>
            {
                TempData["Success"] = "Successfully edited player wallet.";
                TempData["Failure"] = null;
                Log.Local(Owner.Nathan, "Request to player-service-v2 update succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                TempData["Success"] = "Failed to update player wallet.";
                TempData["Failure"] = true;
                Log.Error(Owner.Nathan, "Request to player-service-v2 update failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Patch(out GenericData response, out int code);

        return RedirectToAction("Details", new { id = aid });
    }
}