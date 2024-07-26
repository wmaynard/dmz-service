using System;
using System.Linq;
using Dmz.Filters;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Utilities.JsonTools;

// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("dmz/config"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class DynamicConfigController : DmzController
{
    #region View config
    // Get all config settings
    [HttpGet, Route("settings")]
    public ActionResult Settings()
    {
        Require(Permissions.Config.View);

        return Forward("/config/settings/all");
    }
    #endregion

    #region Modify config
    // Create a new config section
    [HttpPost, Route("section")]
    public ActionResult Section()
    {
        Require(Permissions.Config.Manage);

        ActionResult output = Forward("/config/settings/new");
        
        AuditFilter.UpdateLog(message: $"{Token.Email} created a new dynamic config section: {Optional<string>("name")}");

        return output;
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

        ActionResult output = Forward("/config/settings/value");
        
        string name = Optional<string>("name");
        string key = Optional<string>("key");
        AuditFilter.UpdateLog(
            message: $"{Token.Email} deleted a value in DynamicConfig: {name}.{key}",
            additionalData: Body
        );
        
        return output;
    }

    [HttpPatch, Route("diff")]
    public ActionResult Diff()
    {
        Require(Permissions.Config.ShowDiffs);

        return Forward("/config/diff");
    }
    
    #endregion
}