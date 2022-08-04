using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Models.Config;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Models;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Authorize]
[Route("portal/config")]
public class DynamicConfigController : PortalController
{
    private readonly ApiService           _apiService;
    private readonly AccountService       _accountService;
    private readonly DynamicConfigService _dynamicConfigService;
    private readonly DC2Service           _dc2Service;

    [Route("edit")]
    public async Task<IActionResult> Edit()
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
        bool currentViewConfig = currentPermissions.ViewConfig;
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
        if (currentViewConfig)
        {
            ViewData["CurrentViewConfig"] = currentPermissions.ViewConfig;
        }
        
        // Redirect if not allowed
        if (currentViewConfig == false)
        {
            return View("Error");
        }

        Section[] sections = _dc2Service.GetAdminData();
        ViewData["Data"] = sections;
        
        /*
        string token = _dynamicConfigService.GameConfig.Require<string>("portalToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/config/settings";
        //string requestUrl = PlatformEnvironment.Url("/config/settings");

        if ((string) TempData["Success"] != "Successfully updated dynamic config settings." ||
            (string) TempData["Success"] != "Failed to update dynamic config settings.")
        {
            TempData["UpdateSuccess"] = (string) TempData["Success"];
        }
        else
        {
            TempData["UpdateSuccess"] = null;
        }
        
        TempData["Success"] = "";
        TempData["Failure"] = null;

        _apiService
            .Request(requestUrl + "?game=" + PlatformEnvironment.GameSecret + "&secret=" + PlatformEnvironment.RumbleSecret)
            //.AddParameter(key: "game", value: PlatformEnvironment.GameSecret)
            //.AddParameter(key: "secret", value: PlatformEnvironment.RumbleSecret)
            .AddAuthorization(token)
            .OnSuccess((sender, apiResponse) =>
            {
                TempData["Success"] = "Successfully fetched dynamic config settings.";
                TempData["Failure"] = null;
                Log.Local(Owner.Nathan, "Request to dynamic-config-service succeeded.");
            })
            .OnFailure((sender, apiResponse) =>
            {
                TempData["Success"] = "Failed to fetch dynamic config settings.";
                TempData["Failure"] = true;
                Log.Error(Owner.Nathan, "Request to dynamic-config-service failed.", data: new
                {
                    Response = apiResponse
                });
            })
            .Get(out GenericData response, out int code);

        List<List<string>> data = new List<List<string>>(); // TODO refactor into model
        
        if (response == null)
        {
            TempData["Success"] = "Service response was null.";
            TempData["Failure"] = true;
            
            ViewData["Data"] = data;
            
            return View();
        }

        List<string> names = new List<string>();
        foreach (string name in response.Keys)
        {
            names.Add(name);
        }
        
        List<List<string>> dataEntries = new List<List<string>>();
        
        foreach (GenericData genericData in response.Values)
        {
            string key = genericData.Keys.First();
            string value = genericData.Values.First().ToString();
            List<string> dataEntry = new List<string>
            {
                key,
                value
            };
            dataEntries.Add(dataEntry);
        }

        for (int i = 0; i < names.Count; i++)
        {
            List<string> combinedData = new List<string>
            {
                names[i],
                dataEntries[i][0],
                dataEntries[i][1]
            };
            data.Add(combinedData);
        }

        ViewData["Names"] = names;
        ViewData["Data"] = data;
        */
        
        return View();
    }

    [HttpPost]
    [Route("newSection")]
    public async Task<IActionResult> NewSection(IFormCollection collection)
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
        bool currentViewConfig = currentPermissions.ViewConfig;
        bool currentEditConfig = currentPermissions.EditConfig;
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
        if (currentViewConfig)
        {
            ViewData["CurrentViewConfig"] = currentPermissions.ViewConfig;
        }
        if (currentEditConfig)
        {
            ViewData["CurrentEditConfig"] = currentPermissions.EditConfig;
        }
        
        // Redirect if not allowed
        if (currentEditConfig == false)
        {
            return View("Error");
        }

        string name = collection["name"];
        string friendlyName = collection["friendlyName"];

        string token = _dynamicConfigService.GameConfig.Require<string>("portalToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/config/settings/new";
        //string requestUrl = PlatformEnvironment.Url("/config/settings/new");
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        _apiService
            .Request(requestUrl + "?game=" + PlatformEnvironment.GameSecret + "&secret=" + PlatformEnvironment.RumbleSecret)
            //.AddParameter(key: "game", value: PlatformEnvironment.GameSecret)
            //.AddParameter(key: "secret", value: PlatformEnvironment.RumbleSecret)
            .AddAuthorization(token)
            .SetPayload(new GenericData
            {
                {"name", name},
                {"friendlyName", friendlyName}
            })
            .OnSuccess(((sender, apiResponse) =>
            {
                TempData["Success"] = "Successfully created dynamic config settings.";
                TempData["Failure"] = null;
                Log.Local(Owner.Nathan, "Request to dynamic-config-service update succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                TempData["Success"] = "Failed to create new dynamic config settings.";
                TempData["Failure"] = true;
                Log.Error(Owner.Nathan, "Request to dynamic-config-service update failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Post(out GenericData response, out int code);

        return RedirectToAction("Edit");
    }
    
    [HttpPost]
    [Route("newVariable")]
    public async Task<IActionResult> NewVariable(IFormCollection collection)
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
        bool currentViewConfig = currentPermissions.ViewConfig;
        bool currentEditConfig = currentPermissions.EditConfig;
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
        if (currentViewConfig)
        {
            ViewData["CurrentViewConfig"] = currentPermissions.ViewConfig;
        }
        if (currentEditConfig)
        {
            ViewData["CurrentEditConfig"] = currentPermissions.EditConfig;
        }
        
        // Redirect if not allowed
        if (currentEditConfig == false)
        {
            return View("Error");
        }

        string name = collection["name"];
        string key = collection["key"];
        string value = collection["value"];

        string token = _dynamicConfigService.GameConfig.Require<string>("portalToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/config/settings/update";
        //string requestUrl = PlatformEnvironment.Url("/config/settings/update");
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        _apiService
            .Request(requestUrl + "?game=" + PlatformEnvironment.GameSecret + "&secret=" + PlatformEnvironment.RumbleSecret)
            //.AddParameter(key: "game", value: PlatformEnvironment.GameSecret)
            //.AddParameter(key: "secret", value: PlatformEnvironment.RumbleSecret)
            .AddAuthorization(token)
            .SetPayload(new GenericData
            {
                {"name", name},
                {"key", key},
                {"value", value},
                {"comment", $"Modified in admin portal by user {mongoAccount.Email}."}
            })
            .OnSuccess(((sender, apiResponse) =>
            {
                TempData["Success"] = "Successfully updated dynamic config settings.";
                TempData["Failure"] = null;
                Log.Local(Owner.Nathan, "Request to dynamic-config-service update succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                TempData["Success"] = "Failed to update dynamic config settings.";
                TempData["Failure"] = true;
                Log.Error(Owner.Nathan, "Request to dynamic-config-service update failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Patch(out GenericData response, out int code);

        return RedirectToAction("Edit");
    }

    [HttpPost]
    [Route("delete")]
    public async Task<IActionResult> Delete(IFormCollection collection)
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
        bool currentViewConfig = currentPermissions.ViewConfig;
        bool currentEditConfig = currentPermissions.EditConfig;
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
        if (currentViewConfig)
        {
            ViewData["CurrentViewConfig"] = currentPermissions.ViewConfig;
        }
        if (currentEditConfig)
        {
            ViewData["CurrentEditConfig"] = currentPermissions.EditConfig;
        }
        
        // Redirect if not allowed
        if (currentEditConfig == false)
        {
            return View("Error");
        }

        string name = collection["name"];
        string key = collection["key"];

        string token = _dynamicConfigService.GameConfig.Require<string>("portalToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/config/settings/value";
        //string requestUrl = PlatformEnvironment.Url("/config/settings/value");
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        _apiService
            .Request(requestUrl + "?name=" + name + "&key=" + key)
            //.AddParameter(key: "name", value: name)
            //.AddParameter(key: "key", value: key)
            .AddAuthorization(token)
            .OnSuccess(((sender, apiResponse) =>
            {
                TempData["Success"] = "Successfully updated dynamic config settings.";
                TempData["Failure"] = null;
                Log.Local(Owner.Nathan, "Request to dynamic-config-service update succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                TempData["Success"] = "Failed to update dynamic config settings.";
                TempData["Failure"] = true;
                Log.Error(Owner.Nathan, "Request to dynamic-config-service update failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Delete(out GenericData response, out int code);

        return RedirectToAction("Edit");
    }

    [HttpPost]
    [Route("edit")]
    public async Task<IActionResult> Edit(IFormCollection collection)
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
        bool currentViewConfig = currentPermissions.ViewConfig;
        bool currentEditConfig = currentPermissions.EditConfig;
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
        if (currentViewConfig)
        {
            ViewData["CurrentViewConfig"] = currentPermissions.ViewConfig;
        }
        if (currentEditConfig)
        {
            ViewData["CurrentEditConfig"] = currentPermissions.EditConfig;
        }
        
        // Redirect if not allowed
        if (currentEditConfig == false)
        {
            return View("Error");
        }

        string name = collection["name"];
        string key = collection["key"];
        string value = collection["value"];

        string token = _dynamicConfigService.GameConfig.Require<string>("portalToken");
        string requestUrl = $"{PlatformEnvironment.Optional<string>("PLATFORM_URL").TrimEnd('/')}/config/settings/update";
        //string requestUrl = PlatformEnvironment.Url("/config/settings/update");
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        _apiService
            .Request(requestUrl + "?game=" + PlatformEnvironment.GameSecret + "&secret=" + PlatformEnvironment.RumbleSecret)
            //.AddParameter(key: "game", value: PlatformEnvironment.GameSecret)
            //.AddParameter(key: "secret", value: PlatformEnvironment.RumbleSecret)
            .AddAuthorization(token)
            .SetPayload(new GenericData
            {
                {"name", name},
                {"key", key},
                {"value", value},
                {"comment", $"Modified in admin portal by user {mongoAccount.Email}."}
            })
            .OnSuccess(((sender, apiResponse) =>
            {
                TempData["Success"] = "Successfully updated dynamic config settings.";
                TempData["Failure"] = null;
                Log.Local(Owner.Nathan, "Request to dynamic-config-service update succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                TempData["Success"] = "Failed to update dynamic config settings.";
                TempData["Failure"] = true;
                Log.Error(Owner.Nathan, "Request to dynamic-config-service update failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Patch(out GenericData response, out int code);

        return RedirectToAction("edit");
    }
    
    [Route("showNewSection")]
    public IActionResult ShowNewSectionOverlay()
    {
        TempData["VisibleOverlay"] = true;
        TempData["VisibleSection"] = true;
        TempData["VisibleVariable"] = null;
        TempData["VisibleDelete"] = null;
        
        return RedirectToAction("Edit");
    }
    
    [Route("showNewVariable")]
    public IActionResult ShowNewVariableOverlay(string name)
    {
        TempData["VariableSection"] = name;
        
        TempData["VisibleOverlay"] = true;
        TempData["VisibleSection"] = null;
        TempData["VisibleVariable"] = true;
        TempData["VisibleDelete"] = null;
        
        return RedirectToAction("Edit");
    }
    
    [Route("showDelete")]
    public IActionResult ShowDeleteOverlay(string name, string key, string value)
    {
        TempData["VariableSection"] = name;
        TempData["VariableKey"] = key;
        TempData["VariableValue"] = value;
        
        TempData["VisibleOverlay"] = true;
        TempData["VisibleSection"] = null;
        TempData["VisibleVariable"] = null;
        TempData["VisibleDelete"] = true;
        
        return RedirectToAction("Edit");
    }

    [Route("hideOverlay")]
    public IActionResult HideOverlay()
    {
        TempData["VariableSection"] = null;
        TempData["VariableKey"] = null;
        TempData["VariableValue"] = null;
        
        TempData["VisibleOverlay"] = null;
        TempData["VisibleSection"] = null;
        TempData["VisibleVariable"] = null;
        TempData["VisibleDelete"] = null;
        
        return RedirectToAction("Edit");
    }
}