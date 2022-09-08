using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;

namespace Dmz.Controllers;

[Route("dmz/config"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class DynamicConfigController : DmzController
{
    #region View config
    // Get all config settings
    [HttpGet, Route("settings")]
    public ActionResult Settings()
    {
        Require(Permissions.Config.View_Page);

        return Forward("/config/settings/all");
    }
    #endregion

    #region Modify config
    // Create a new config section
    [HttpPost, Route("section")]
    public ActionResult Section()
    {
        Require(Permissions.Config.Manage);

        return Forward("/config/settings/new");
    }
  
    // Update a section
    // Used for both adding a new variable and updating an existing variable
    [HttpPatch, Route("update")]
    public ActionResult Update()
    {
        Require(Permissions.Config.Manage);

        return Forward("/config/settings/update");
    }
  
    // Delete a variable
    [HttpDelete, Route("delete")]
    public ActionResult Delete()
    {
        Require(Permissions.Config.Delete);

        return Forward("/config/settings/value");
    }
    #endregion
}