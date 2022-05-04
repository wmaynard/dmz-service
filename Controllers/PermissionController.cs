using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Services;
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
    public async Task<IActionResult> Account(string accountId)
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
        
        Account user = _accountService.Get(accountId);
        
        ViewData["Account"] = user;

        return View();
    }

    [HttpPost]
    [Route("account")]
    public async Task<IActionResult> Account(string accountId, List<string> roles)
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
        
        Account user = _accountService.Get(accountId);
        
        _accountService.UpdateRoles(user, roles);
        
        ViewData["Account"] = user;
        
        return View();
    }
    
    [Route("health")]
    public override ActionResult HealthCheck() => Ok(_apiService.HealthCheckResponseObject, _dynamicConfigService.HealthCheckResponseObject);
}