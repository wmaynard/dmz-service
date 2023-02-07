using System;
using System.Linq;
using Dmz.Filters;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

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

        const string PREVIOUS = "previousValue";
        const string PREVIOUS_COMMENT = "previousComment";
        string name = Optional<string>("name");
        string key = Optional<string>("key");
        object value = Optional<object>("value");
        object comment = Optional<object>("comment");
        
        
        RumbleJson data = Body.Copy();
        string[] names = Enum
            .GetValues<Audience>()
            .Select(aud => aud.GetDisplayName())
            .ToArray();
        Audience audience = Enum
            .GetValues<Audience>()
            .Where(aud => aud.GetDisplayName() != null)
            .FirstOrDefault(aud => aud.GetDisplayName() == Optional<string>("name"));

        RumbleJson previous = new RumbleJson();
        if (audience != default)
            previous = DynamicConfig.GetValuesFor(audience) ?? new RumbleJson();

        ActionResult output = Forward("/config/settings/update");

        if (!previous.ContainsKey(key))
        {
            string fallback = DynamicConfig.Optional<string>(key);
            data[PREVIOUS] = fallback;
            AuditFilter.UpdateLog(
                message: $"{Token.Email} updated DynamicConfig: {name}.{key}",
                data: data
            );
        }
        else if (!value.Equals(previous[key]))
        {
            data[PREVIOUS] = previous[key];
            AuditFilter.UpdateLog(
                message: $"{Token.Email} changed a value in DynamicConfig: {name}.{key}",
                data: data
            );
        }
        else
        {
            data[PREVIOUS_COMMENT] = comment;
            AuditFilter.UpdateLog(
                message: $"{Token.Email} changed a comment in DynamicConfig: {name}.{key}",
                data: data
            );
        }

        return output;
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
            data: Body
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