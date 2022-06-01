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

namespace TowerPortal.Controllers;

[Authorize]
[Route("permission")]
public class PermissionController : PlatformController
{
    private readonly ApiService _apiService;
    private readonly DynamicConfigService _dynamicConfigService;
    private readonly AccountService _accountService;

    [Route("list")]
    public async Task<IActionResult> List()
    {
        ViewData["Message"] = "User list";
        TempData["Success"] = null;
        TempData["Failure"] = null;

        // Checking access permissions
        Account account = Models.Account.FromGoogleClaims(User.Claims); // Models required for some reason?
        Account mongoAccount = _accountService.FindOne(mongo => mongo.Email == account.Email);
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
        if (currentAdmin == false && currentManagePermissions == false)
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
        Account account = Models.Account.FromGoogleClaims(User.Claims); // Models required for some reason?
        Account mongoAccount = _accountService.FindOne(mongo => mongo.Email == account.Email);
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
        if (currentAdmin == false && currentManagePermissions == false)
        {
            return View("Error");
        }
        
        ViewData["Environment"] = PlatformEnvironment.Optional<string>(key: "RUMBLE_DEPLOYMENT");
        
        Account user = _accountService.Get(id);
        ViewData["Permissions"] = user.Permissions; // Inconsistent with others of same name, since this is needed for permissions page

        TempData["AccountId"] = id;
        ViewData["Account"] = user.Email;

        return View();
    }

    [HttpPost]
    [Route("account")]
    public async Task<IActionResult> Account(string id, string managePermissions, string viewPlayer, string editPlayer, string viewMailbox, string editMailbox, string viewToken, string editToken)
    {
        // Checking access permissions
        Account account = Models.Account.FromGoogleClaims(User.Claims); // Models required for some reason?
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
        if (currentAdmin == false && currentManagePermissions == false)
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
            else
            {
                user.Permissions.ManagePermissions = false;
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

