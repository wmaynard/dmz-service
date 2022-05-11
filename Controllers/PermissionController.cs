using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        string admin = _accountService.CheckPermissions(account, "admin");
        string viewPlayer = _accountService.CheckPermissions(account, "viewPlayer");
        string viewMailbox = _accountService.CheckPermissions(account, "viewMailbox");
        // Tab view permissions
        ViewData["Admin"] = admin;
        ViewData["ViewPlayer"] = viewPlayer;
        ViewData["ViewMailbox"] = viewMailbox;
        
        // Redirect if not allowed
        if (admin == null)
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
        string admin = _accountService.CheckPermissions(account, "admin");
        string viewPlayer = _accountService.CheckPermissions(account, "viewPlayer");
        string viewMailbox = _accountService.CheckPermissions(account, "viewMailbox");
        // Tab view permissions
        ViewData["Admin"] = admin;
        ViewData["ViewPlayer"] = viewPlayer;
        ViewData["ViewMailbox"] = viewMailbox;
        
        // Redirect if not allowed
        if (admin == null)
        {
            return View("Error");
        }

        List<String> environments = new List<String>();
        environments.Add("107"); // currently hard coded 107
        environments.Add("207"); // currently hard coded 207
        environments.Add("217"); // currently hard coded 217
        environments.Add("227"); // currently hard coded 227
        ViewData["Envs"] = environments;
        
        Account user = _accountService.Get(id);
        
        List<string> currentRoles = user.Roles;

        // TODO refactor, figure out how to do without needing to hardcode
        ViewData["ViewPlayer107"] = null;
        if (currentRoles.Contains("viewPlayer107"))
        {
            ViewData["ViewPlayer107"] = "on";
        }
        ViewData["ViewPlayer207"] = null;
        if (currentRoles.Contains("viewPlayer207"))
        {
            ViewData["ViewPlayer207"] = "on";
        }
        ViewData["ViewPlayer217"] = null;
        if (currentRoles.Contains("viewPlayer217"))
        {
            ViewData["ViewPlayer217"] = "on";
        }
        ViewData["ViewPlayer227"] = null;
        if (currentRoles.Contains("viewPlayer227"))
        {
            ViewData["ViewPlayer227"] = "on";
        }
        ViewData["EditPlayer107"] = null;
        if (currentRoles.Contains("editPlayer107"))
        {
            ViewData["EditPlayer107"] = "on";
        }
        ViewData["EditPlayer207"] = null;
        if (currentRoles.Contains("editPlayer207"))
        {
            ViewData["EditPlayer207"] = "on";
        }
        ViewData["EditPlayer217"] = null;
        if (currentRoles.Contains("editPlayer217"))
        {
            ViewData["EditPlayer217"] = "on";
        }
        ViewData["EditPlayer227"] = null;
        if (currentRoles.Contains("editPlayer227"))
        {
            ViewData["EditPlayer227"] = "on";
        }
        ViewData["ViewMailbox107"] = null;
        if (currentRoles.Contains("viewMailbox107"))
        {
            ViewData["ViewMailbox107"] = "on";
        }
        ViewData["ViewMailbox207"] = null;
        if (currentRoles.Contains("viewMailbox207"))
        {
            ViewData["ViewMailbox207"] = "on";
        }
        ViewData["ViewMailbox217"] = null;
        if (currentRoles.Contains("viewMailbox217"))
        {
            ViewData["ViewMailbox217"] = "on";
        }
        ViewData["ViewMailbox227"] = null;
        if (currentRoles.Contains("viewMailbox227"))
        {
            ViewData["ViewMailbox227"] = "on";
        }
        ViewData["EditMailbox107"] = null;
        if (currentRoles.Contains("editMailbox107"))
        {
            ViewData["EditMailbox107"] = "on";
        }
        ViewData["EditMailbox207"] = null;
        if (currentRoles.Contains("editMailbox207"))
        {
            ViewData["EditMailbox207"] = "on";
        }
        ViewData["EditMailbox217"] = null;
        if (currentRoles.Contains("editMailbox217"))
        {
            ViewData["EditMailbox217"] = "on";
        }
        ViewData["EditMailbox227"] = null;
        if (currentRoles.Contains("editMailbox227"))
        {
            ViewData["EditMailbox227"] = "on";
        }
        
        TempData["AccountId"] = id;
        ViewData["Account"] = user.Email;

        return View();
    }

    [HttpPost]
    [Route("account")]
    public async Task<IActionResult> Account(string id,
        string viewPlayer107, string editPlayer107, string viewMailbox107, string editMailbox107,
        string viewPlayer207, string editPlayer207, string viewMailbox207, string editMailbox207,
        string viewPlayer217, string editPlayer217, string viewMailbox217, string editMailbox217,
        string viewPlayer227, string editPlayer227, string viewMailbox227, string editMailbox227
        )
        // Need to find a way to not have to manually put in each separate permission in parameters
        // Seems like name field in views has to be hardcoded
    {
        // Checking access permissions
        Account account = Models.Account.FromGoogleClaims(User.Claims); // Models required for some reason?
        string admin = _accountService.CheckPermissions(account, "admin");
        string viewPlayer = _accountService.CheckPermissions(account, "viewPlayer");
        string viewMailbox = _accountService.CheckPermissions(account, "viewMailbox");
        // Tab view permissions
        ViewData["Admin"] = admin;
        ViewData["ViewPlayer"] = viewPlayer;
        ViewData["ViewMailbox"] = viewMailbox;
        
        // Redirect if not allowed
        if (admin == null)
        {
            return View("Error");
        }

        Account user = _accountService.Get(id);

        List<string> roles = new List<string>();
        
        if (user.Roles.Contains("admin107")) // prevent remove own admin
        // TODO replace with actual env, hardcoded env for now
        {
            roles.Add("admin107");
        }
        
        // Need to find a way to not have to manually put in each separate permission in parameters
        // Seems like name field in views has to be hardcoded
        if (viewPlayer107 != null)
        {
            roles.Add("viewPlayer107");
        }
        if (viewPlayer207 != null)
        {
            roles.Add("viewPlayer207");
        }
        if (viewPlayer217 != null)
        {
            roles.Add("viewPlayer217");
        }
        if (viewPlayer227 != null)
        {
            roles.Add("viewPlayer227");
        }
        if (editPlayer107 != null)
        {
            roles.Add("editPlayer107");
        }
        if (editPlayer207 != null)
        {
            roles.Add("editPlayer207");
        }
        if (editPlayer217 != null)
        {
            roles.Add("editPlayer217");
        }
        if (editPlayer227 != null)
        {
            roles.Add("editPlayer227");
        }
        if (viewMailbox107 != null)
        {
            roles.Add("viewMailbox107");
        }
        if (viewMailbox207 != null)
        {
            roles.Add("viewMailbox207");
        }
        if (viewMailbox217 != null)
        {
            roles.Add("viewMailbox217");
        }
        if (viewMailbox227 != null)
        {
            roles.Add("viewMailbox227");
        }
        if (editMailbox107 != null)
        {
            roles.Add("editMailbox107");
        }
        if (editMailbox207 != null)
        {
            roles.Add("editMailbox207");
        }
        if (editMailbox217 != null)
        {
            roles.Add("editMailbox217");
        }
        if (editMailbox227 != null)
        {
            roles.Add("editMailbox227");
        }

        try
        {
            _accountService.UpdateRoles(user, roles);
            TempData["Success"] = "Successfully updated roles for user.";
            TempData["Failure"] = null;
        }
        catch (Exception e)
        {
            Log.Error(owner: Owner.Nathan, message: "Failed to update roles for portal user.", data: e.Message);
            TempData["Success"] = null;
            TempData["Failure"] = "Failed to update roles for user.";
        }
        
        ViewData["Account"] = user.Email;
        
        return RedirectToAction("Account", new { id = id });
    }
    
    [Route("health")]
    public override ActionResult HealthCheck() => Ok(_apiService.HealthCheckResponseObject, _dynamicConfigService.HealthCheckResponseObject);
}