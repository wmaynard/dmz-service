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
using Rumble.Platform.Common.Services;
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
        RumbleJson param = new();
        foreach (string key in context.HttpContext.Request.Query.Keys)
            param[key] = context.HttpContext.Request.Query[key];

        if (param.Any())
            context.HttpContext.Items[KEY_PARAMETERS] = param;
        
        if (context.GetControllerAttributes<NoAuth>().Any())
        {
            Log.Verbose(Owner.Default, message: "NoAuth attribute found on endpoint; Permissions cannot be loaded.");
            return;
        }

        if (Token == null || !Token.IsAdmin)
            return;
        

        // If the token is an admin token, it needs to load permissions.  Permissions are not applicable to player tokens.
        AccountService _accounts = PlatformService.Require<AccountService>();
        Account account = _accounts.FromToken(Token, loadRoles: true);
        context.HttpContext.Items[KEY_PERMISSIONS] = account.Permissions;
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}