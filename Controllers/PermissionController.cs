using System;
using System.Collections.Generic;
using System.Linq;
using Dmz.Models.Permissions;
using Dmz.Models.Portal;
using Dmz.Services;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Utilities.JsonTools;

// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("dmz/permissions"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class PermissionController : DmzController
{
    #pragma warning disable
private readonly AccountService _accounts;
private readonly RoleService    _roles;
    #pragma warning restore
    
    #region Accounts
    // Get account info
    [HttpGet, Route("accounts")]
    public ActionResult ListAccounts()
    {
        Require(Permissions.Portal.ManagePermissions);

        string accountId = Optional<string>("id");

        return Ok(new RumbleJson
        {
            { "accounts", string.IsNullOrWhiteSpace(accountId)
                ? _accounts.All()
                : _accounts.FromId(accountId)
            }
        });
    }
    
    // Update account permissions & Roles
    // TODO: This method should be accepting permission-related classes as parameters, not a ton of strings. 
    [HttpPut, Route("update")]
    public ActionResult UpdatePermissions()
    {
        Require(Permissions.Portal.ManagePermissions);

        string id = Require<string>("id");
        string[] roleIds = Optional<string[]>("roleIds");
        RumbleJson permissions = Optional<RumbleJson>(Account.FRIENDLY_KEY_PERMISSIONS);
        Passport passport = Passport.FromPermissionSet(permissions);
        
        _roles.EnsureAllRolesExist(roleIds);

        if (!_accounts.UpdatePassport(id, roleIds, passport))
            throw new PlatformException("Could not update account permissions.  Either nothing changed or the account doesn't exist.");
        
        return Ok();
    }
    #endregion Accounts
    
    #region Roles
    // Get roles
    [HttpGet, Route("roles")]
    public ActionResult GetRoles()
    {
        Require(Permissions.Portal.ManagePermissions);

        return Ok(new RumbleJson
        {
            { "roles", _roles.List() }
        });
    }
    
    // New role
    [HttpPost, Route("roles/create")]
    public ActionResult AddRole()
    {
        Require(Permissions.Portal.ManagePermissions);
        
        string name = Require<string>("name");
        RumbleJson permissions = Require<RumbleJson>(Account.FRIENDLY_KEY_PERMISSIONS);
        Passport passport = Passport.FromPermissionSet(permissions);
        
        Role role = new()
        {
            Name = name,
            Permissions = passport
        };
        _roles.EnsureNameNotTaken(name);
        _roles.Insert(role);

        return Ok(role);
    }
    
    // Edit role
    [HttpPut, Route("roles/update")]
    public ActionResult EditRole()
    {
        Require(Permissions.Portal.ManagePermissions);

        string id = Require<string>("id");

        if (string.IsNullOrWhiteSpace(id) || !id.CanBeMongoId())
            throw new PlatformException("Invalid role ID");
        
        _roles.EnforceIdNotSuperuser(id);
        RumbleJson permissions = Optional<RumbleJson>(Account.FRIENDLY_KEY_PERMISSIONS);
        Passport passport = Passport.FromPermissionSet(permissions);
        _roles.UpdatePermissions(id, passport);

        return Ok();
    }
    
    // Delete role
    [HttpDelete, Route("roles/delete")]
    public ActionResult DeleteRole()
    {
        Require(Permissions.Portal.ManagePermissions);

        string id = Require<string>("id");

        _roles.EnforceIdNotSuperuser(id);
        _accounts.RemoveRole(id);
        _roles.Delete(id);

        return Ok();
    }
    #endregion Roles
}
