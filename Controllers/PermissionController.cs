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
[Route("portal/permission")]
public class PermissionController : PortalController
{
    private readonly DynamicConfigService _dynamicConfigService;
    private readonly AccountService _accountService;

    [Route("list")]
    public async Task<IActionResult> List()
    {
        ViewData["Message"] = "User list";
        TempData["Success"] = null;
        TempData["Failure"] = null;

        // Checking access permissions
        if (!Permissions.Portal.ManagePermissions)
        {
            return View("Error");
        }

        List<Account> users = _accountService.GetAllAccounts();

        List<List<string>> userData = new List<List<string>>();
        foreach (Account user in users)
        {
            List<string> userInfo = new List<string>();
            userInfo.Add(user.Id);
            userInfo.Add(user.Email);
            userInfo.Add(user.FirstName);
            userInfo.Add(user.FamilyName);
            userData.Add(userInfo);
        }

        ViewData["Users"] = userData;

        return View();
    }

    [Route("account")]
    public async Task<IActionResult> Account(string id)
    {
        // Checking access permissions
        if (!Permissions.Portal.ManagePermissions)
        {
            return View("Error");
        }
        
        ViewData["Environment"] = PlatformEnvironment.Optional<string>(key: "RUMBLE_DEPLOYMENT");
        
        Account user = _accountService.Get(id);
        
        // This is used because there are two sets of permissions to look at TODO perhaps will need another filter to access this more easily
        ViewData["UserPermissions"] = user.Permissions;

        TempData["AccountId"] = id;
        ViewData["Account"] = user.Email;

        return View();
    }

    // TODO: This method should be accepting permission-related classes as parameters, not a ton of strings. 
    [HttpPost]
    [Route("account")]
    public async Task<IActionResult> UpdatePermissions(string id)
    {
        // Checking access permissions
        if (!Permissions.Portal.ManagePermissions)
            return View("Error");
        
        // TODO: Will to parse the Body into a passport

        Account user = _accountService.Get(id);

        try
        {
            // user.Permissions.Portal.ManagePermissions = managePermissions != null;
            // user.Permissions.Player.View_Page = viewPlayer != null;
            // user.Permissions.Player.Edit = editPlayer != null;
            // user.Permissions.Mail.View_Page = viewMailbox != null;
            // user.Permissions.Mail.Edit = editMailbox != null;
            // user.Permissions.Token.View_Page = viewToken != null;
            // user.Permissions.Token.Edit = editToken != null;
            // user.Permissions.Config.View_Page = viewConfig != null;
            // user.Permissions.Config.Edit = editConfig != null;
            
            _accountService.Update(user);
            
            TempData["Success"] = "Successfully updated permissions for user.";
            TempData["Failure"] = null;
        }
        catch (Exception e)
        {
            Log.Error(owner: Owner.Nathan, message: "Failed to update permissions for portal user.", data: e.Message);
            TempData["Success"] = null;
            TempData["Failure"] = "Failed to update permissions for user.";
        }
        
        ViewData["Account"] = user.Email;
        
        return RedirectToAction("Account", new { id = id });
    }
}

