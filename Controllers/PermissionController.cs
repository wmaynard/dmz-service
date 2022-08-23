using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Models;
using TowerPortal.Models.Permissions;
using TowerPortal.Models.Portal;
using TowerPortal.Services;

namespace TowerPortal.Controllers;

[Route("portal/permissions"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class PermissionController : PortalController
{
  #pragma warning disable
    private readonly AccountService _accountService;
  #pragma warning restore
  
  #region List
  // TODO is search needed?
  
  // List all accounts
  [HttpGet, Route("list")]
  public ActionResult List()
  {
    Require(Permissions.Portal.ManagePermissions);
    
    IEnumerable<Account> accounts = _accountService.List();

    return Ok(accounts);
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
  public ActionResult UpdatePermissions(string id, GenericData data)
  {
    Require(Permissions.Portal.ManagePermissions);
    
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
