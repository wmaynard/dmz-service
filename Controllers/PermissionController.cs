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
        {
            return Ok();
        }

        try
        {
            Log.Local(Owner.Will, $"Updated {sum} values.");
            if (_accountService.UpdatePassport(id, displayedUserPermissions) != 1)
            {
                throw new PlatformException(message: "Unable to update permissions.");
            }
        }
        catch (Exception e)
        {
            Log.Error(owner: Owner.Nathan, message: "Failed to update permissions for portal user.", data: e.Message);
        }

        return Ok();
    }
    #endregion
}
