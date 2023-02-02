using System;
using System.Collections.Generic;
using System.Linq;
using Dmz.Models.Permissions;
using Dmz.Models.Portal;
using Dmz.Services;
using Dmz.Utilities;
using Microsoft.AspNetCore.Mvc.Filters;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Filters;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Filters;

public class PermissionsFilter : PlatformFilter, IActionFilter
{
    public const string KEY_PERMISSIONS = "DmzPermissions";
    public const string KEY_PARAMETERS = "QueryParameters";
    public const string KEY_HTTP_METHOD = "RequestMethod";
    
    
    /// <summary>
    /// This executes before every single controller endpoint.  We need to check the Google permissions
    /// for every endpoint, so rather than repeat the code in every single method, this instead allows us to remove
    /// a ton of boilerplate.
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Prepare the HTTP method for forwarding.
        context.HttpContext.Items[KEY_HTTP_METHOD] = context.HttpContext.Request.Method;
        
        // Prepare the query parameters for forwarding.
        RumbleJson param = new RumbleJson();
        foreach (string key in context.HttpContext.Request.Query.Keys)
            param[key] = context.HttpContext.Request.Query[key];

        if (param.Any())
            context.HttpContext.Items[KEY_PARAMETERS] = param;
        
        if (context.GetControllerAttributes<NoAuth>().Any())
        {
            Log.Local(Owner.Default, message: "NoAuth attribute found on endpoint; Permissions cannot be loaded.", emphasis: Log.LogType.WARN);
            return;
        }

        // If the token is an admin token, it needs to load permissions.  Permissions are not applicable to player tokens.
        if (ContextHelper.Token != null && ContextHelper.Token.IsAdmin)
        {
            if (!GetService(out AccountService accountService))
            {
                Log.Local(Owner.Will, "Account Service was null; returning.", emphasis: Log.LogType.ERROR);
                return;
            }

            // Prepare the current user's permissions to be checked by controllers.
            try
            {
                Account account = accountService.FindByToken(ContextHelper.Token);
                Passport accountPassport = account.Permissions;
                Passport combined = accountPassport.Copy();
                
                List<Role> roles = account.Roles;
                
                if (account.Roles == null)
                {
                    account.InitPropertyRole();
                    accountService.Update(account);
                    
                    roles = account.Roles;
                }
                
                foreach (Role role in roles)
                {
                    Passport rolePassport = role.Permissions;
                    foreach (PermissionGroup permissionGroup in combined)
                    {
                        foreach (PermissionGroup rolePermissionGroup in rolePassport)
                        {
                            if (permissionGroup.Name == rolePermissionGroup.Name)
                            {
                                permissionGroup.Merge(rolePermissionGroup);
                            }
                        }
                    }
                }

                context.HttpContext.Items[KEY_PERMISSIONS] = combined;
            }
            catch (Exception e)
            {
                Log.Error(Owner.Will, "Unable to load permissions for a user.", exception: e);
            }
        }
        else
            Log.Local(Owner.Default, message: "Token is not an admin; it must be a player so permissions are not relevant.");
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}