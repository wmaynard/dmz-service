using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Models.Config;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Authorize]
[Route("portal/config")]
public class DynamicConfigController : PortalController
{
    private readonly AccountService       _accountService;
    private readonly DynamicConfigService _dynamicConfigService;

    [Route("edit")]
    public async Task<IActionResult> Edit()
    {
        // Checking access permissions
        if (!UserPermissions.Config.View_Page)
        {
            return View("Error");
        }

        Section[] sections = await _dc2Service.GetAdminDataAsync();
        ViewData["Data"] = sections;
        
        return View();
    }

    [HttpPost]
    [Route("newSection")]
    public async Task<IActionResult> NewSection(IFormCollection collection)
    {
        // Checking access permissions
        if (!UserPermissions.Config.View_Page || !UserPermissions.Config.Edit)
        {
            return View("Error");
        }

        string name = collection["name"];
        string friendlyName = collection["friendlyName"];

        string token = _dynamicConfigService.GameConfig.Require<string>("portalToken");
        string requestUrl = PlatformEnvironment.Url("/config/settings/new");
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        _apiService
            .Request(requestUrl)
            .AddParameter(key: "game", value: PlatformEnvironment.GameSecret)
            .AddParameter(key: "secret", value: PlatformEnvironment.RumbleSecret)
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
        if (!UserPermissions.Config.View_Page || !UserPermissions.Config.Edit)
        {
            return View("Error");
        }

        string name = collection["name"];
        string key = collection["key"];
        string value = collection["value"];

        string token = _dynamicConfigService.GameConfig.Require<string>("portalToken");
        string requestUrl = PlatformEnvironment.Url("/config/settings/update");
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        _apiService
            //.Request(requestUrl + "?game=" + PlatformEnvironment.GameSecret + "&secret=" + PlatformEnvironment.RumbleSecret)
            .Request(requestUrl)
            .AddParameter(key: "game", value: PlatformEnvironment.GameSecret)
            .AddParameter(key: "secret", value: PlatformEnvironment.RumbleSecret)
            .AddAuthorization(token)
            .SetPayload(new GenericData
            {
                {"name", name},
                {"key", key},
                {"value", value},
                {"comment", $"Modified in admin portal by user {User?.Identity?.Name}."}
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
        if (!UserPermissions.Config.View_Page || !UserPermissions.Config.Edit)
        {
            return View("Error");
        }

        string name = collection["name"];
        string key = collection["key"];

        string token = _dynamicConfigService.GameConfig.Require<string>("portalToken");
        string requestUrl = PlatformEnvironment.Url("/config/settings/value");
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        _apiService
            .Request(requestUrl)
            .AddParameter(key: "name", value: name)
            .AddParameter(key: "key", value: key)
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
        if (!UserPermissions.Config.View_Page || !UserPermissions.Config.Edit)
        {
            return View("Error");
        }

        string name = collection["name"];
        string key = collection["key"];
        string value = collection["value"];

        string token = _dynamicConfigService.GameConfig.Require<string>("portalToken");
        string requestUrl = PlatformEnvironment.Url("/config/settings/update");
        
        TempData["Success"] = "";
        TempData["Failure"] = null;
        
        _apiService
            .Request(requestUrl)
            .AddAuthorization(token)
            .SetPayload(new GenericData
            {
                {"name", name},
                {"key", key},
                {"value", value},
                {"comment", $"Modified in admin portal by user {User?.Identity?.Name}."}
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