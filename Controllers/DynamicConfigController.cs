using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;

namespace TowerPortal.Controllers;

[Route("portal/config"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class DynamicConfigController : PortalController
{
  // Fetches all config settings
  [HttpGet, Route("settings")]
  public ActionResult Settings()
  {
    Require(Permissions.Config.View_Page);

    // await _dc2Service.GetAdminDataAsync()
    return Forward("/config/settings/all");
  }

  // Sends a request to create a new config section
  [HttpPost, Route("section")]
  public ActionResult Section()
  {
    Require(Permissions.Config.Manage);

    return Forward("/config/settings/new");
  }
  
  // Sends a request to update a section
  // Used for both adding a new variable and updating an existing variable
  [HttpPatch, Route("update")]
  public ActionResult Update()
  {
    Require(Permissions.Config.Manage);

    return Forward("/config/settings/update");
  }
  
  // Sends a request to delete a variable
  [HttpDelete, Route("delete")]
  public ActionResult Delete()
  {
    Require(Permissions.Config.Manage);

    return Forward("config/settings/value");
  }
}