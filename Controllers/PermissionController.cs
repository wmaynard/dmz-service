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
        if (!UserPermissions.Admin && !UserPermissions.ManagePermissions)
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
        if (!UserPermissions.Admin && !UserPermissions.ManagePermissions)
        {
            return View("Error");
        }
        
        // Following is for the current user's permissions, since permissions on this page is used for the user being modified.
        // TODO refactor as this is inconsistent with others of the same use
        ViewData["CurrentAdmin"] = UserPermissions.Admin;
        ViewData["CurrentManagePermissions"] = UserPermissions.ManagePermissions;
        
        ViewData["Environment"] = PlatformEnvironment.Optional<string>(key: "RUMBLE_DEPLOYMENT");
        
        Account user = _accountService.Get(id);
        ViewData["Permissions"] = user.Permissions; // TODO Note above

        TempData["AccountId"] = id;
        ViewData["Account"] = user.Email;

        return View();
    }

    [HttpPost]
    [Route("account")]
    public async Task<IActionResult> Account(string id, string managePermissions, string viewPlayer, string editPlayer, string viewMailbox, string editMailbox, string viewToken, string editToken, string viewConfig, string editConfig)
    {
        // Checking access permissions
        if (!UserPermissions.Admin && !UserPermissions.ManagePermissions)
        {
            return View("Error");
        }

        Account user = _accountService.Get(id);

        try
        {
            if (managePermissions != null)
            {
                user.Permissions.ManagePermissions = true;
            }
            if (viewPlayer != null)
            {
                user.Permissions.ViewPlayer = true;
            }
            else
            {
                user.Permissions.ViewPlayer = false;
            }
            if (editPlayer != null)
            {
                user.Permissions.EditPlayer = true;
            }
            else
            {
                user.Permissions.EditPlayer = false;
            }
            if (viewMailbox != null)
            {
                user.Permissions.ViewMailbox = true;
            }
            else
            {
                user.Permissions.ViewMailbox = false;
            }
            if (editMailbox != null)
            {
                user.Permissions.EditMailbox = true;
            }
            else
            {
                user.Permissions.EditMailbox = false;
            }
            if (viewToken != null)
            {
                user.Permissions.ViewToken = true;
            }
            else
            {
                user.Permissions.ViewToken = false;
            }
            if (editToken != null)
            {
                user.Permissions.EditToken = true;
            }
            else
            {
                user.Permissions.EditToken = false;
            }
            if (viewConfig != null)
            {
                user.Permissions.ViewConfig = true;
            }
            else
            {
                user.Permissions.ViewConfig = false;
            }
            if (editConfig != null)
            {
                user.Permissions.EditConfig = true;
            }
            else
            {
                user.Permissions.EditConfig = false;
            }
            
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

