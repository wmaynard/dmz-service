using System;
using System.Collections.Generic;
using System.Linq;
using Dmz.Models.Permissions;
using Dmz.Models.Portal;
using Dmz.Services;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Utilities;

// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("dmz/permissions"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class PermissionController : DmzController
{
    #pragma warning disable
        private readonly AccountService _accountService;
        private readonly RoleService    _roleService;
    #pragma warning restore
    
    #region List
    // TODO search if user base becomes large
    
    // List all accounts
    [HttpGet, Route("list")]
    public ActionResult List()
    {
        Require(Permissions.Portal.ManagePermissions);
      
        IEnumerable<Account> accounts = _accountService.List();

        return Ok(new {Accounts = accounts});
    }
    
    // Search by role
    [HttpGet, Route("search/role")]
    public ActionResult SearchRole(string name)
    {
        Require(Permissions.Portal.ManagePermissions);

        List<Account> accounts = _accountService.FindByRole(name);

        return Ok(new {Accounts = accounts});
    }
    
    // Search by permission
    [HttpGet, Route("search/permission")]
    public ActionResult SearchPermission(string name)
    {
        Require(Permissions.Portal.ManagePermissions);

        List<Account> accounts = _accountService.FindByPermission(name);
        
        return Ok(new {Accounts = accounts});
    }
    
    #endregion
    
    #region Account page
    // Get account info
    [HttpGet, Route("account")]
    public ActionResult Account()
    {
        Require(Permissions.Portal.ManagePermissions);

        string accountId = Require<string>(key: "id");
        Account account = _accountService.Get(accountId);

        return Ok(account);
    }
    
    // Update account permissions
    // TODO: This method should be accepting permission-related classes as parameters, not a ton of strings. 
    [HttpPatch, Route("account")]
    public ActionResult UpdatePermissions()
    {
        Require(Permissions.Portal.ManagePermissions);

        string id = Require<string>(key: "id");

        // Differentiate this from the regular Permissions property, which refers to the current user - not the one displayed on screen.
        Passport displayedUserPermissions = _accountService.FindById(id).Permissions;
        int sum = displayedUserPermissions.Sum(group => group.UpdateFromValues(Body));

        // Nothing was changed; no reason to do anything further.
        if (sum == 0)
            return Ok();

        if (_accountService.UpdatePassport(id, displayedUserPermissions) != 1)
            throw new PlatformException(message: "Unable to update permissions.");

        return Ok(new {Message = $"Updated {sum} values."});
    }
    
    // Modify roles
    [HttpPatch, Route("account/roles")]
    public ActionResult AccountRoles()
    {
        Require(Permissions.Portal.ManagePermissions);

        string id = Require<string>("id");
        List<string> roleNames = Require<List<string>>("roles");

        Account account = _accountService.FindById(id);

        try
        {
            // TODO: This really needs to be a single mongo query
            account.Roles = roleNames
                .Select(role => _roleService.FindByName(role))
                .ToList();
        }
        catch (Exception e)
        {
            throw new PlatformException(message: $"An attempt was made to add a non-existent role.", inner: e)
            {
                Data = { {"roles", roleNames } }
            };
        }

        _accountService.Update(account);

        return Ok(account.Roles);
    }
    #endregion
    
    #region Roles
    // Get roles
    [HttpGet, Route("roles")]
    public ActionResult GetRoles()
    {
        Require(Permissions.Portal.ManagePermissions);

        List<Role> roles = (List<Role>) _roleService.List();
        return Ok(new {Roles = roles});
    }
    
    // New role
    [HttpPost, Route("roles/add")]
    public ActionResult AddRole()
    {
        Require(Permissions.Portal.ManagePermissions);

        string name = Require<string>(key: "name");

        if (_roleService.FindByName(name) != null)
        {
            throw new PlatformException(message: "Role already exists.");
        }

        Passport passport = new Passport(Passport.PassportType.Readonly);
        
        int sum = passport.Sum(group => group.UpdateFromValues(Body));

        Role role = new Role(name: name, passport: passport);
        _roleService.Create(role);

        return Ok(message: $"{sum} permissions were added for new role {name}.");
    }
    
    // Edit role
    [HttpPatch, Route("roles/edit")]
    public ActionResult EditRole()
    {
        Require(Permissions.Portal.ManagePermissions);
        
        string name = Require<string>(key: "name");
        
        Role role = _roleService.FindByName(name);

        if (role == null)
        {
            throw new PlatformException(message: "Role does not exist.");
        }

        int sum = role.Permissions.Sum(group => group.UpdateFromValues(Body));
        
        _roleService.Update(role); // role definition in mongo
        _accountService.UpdateEditedRole(role); // for roles already in accounts

        return Ok(message: $"{sum} permissions were edited for role {name}.");
    }
    
    // Delete role
    [HttpDelete, Route("roles/delete")]
    public ActionResult DeleteRole()
    {
        Require(Permissions.Portal.ManagePermissions);

        string name = Require<string>(key: "name");

        Role role = _roleService.FindByName(name);
        
        if (role == null)
        {
            throw new PlatformException(message: "Role does not exist.");
        }
        
        _roleService.Delete(role);
        _accountService.RemoveDeletedRole(role.Name);

        return Ok(message: $"Role {name} has been deleted");
    }
    #endregion
}
