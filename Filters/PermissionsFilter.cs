using System;
using System.Linq;
using Dmz.Services;
using Dmz.Utilities;
using Microsoft.AspNetCore.Mvc.Filters;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Filters;
using Rumble.Platform.Common.Utilities;

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
        if (context.GetControllerAttributes<NoAuth>().Any())
        {
            Log.Local(Owner.Default, message: "NoAuth attribute found on endpoint; Permissions cannot be loaded.", emphasis: Log.LogType.WARN);
            return;
        }
        
        if (!GetService(out AccountService accountService))
        {
            Log.Local(Owner.Will, "Account Service was null, returning.", emphasis: Log.LogType.ERROR);
            return;
        }

        // Prepare the current user's permissions to be checked by controllers.
        try
        {
            context.HttpContext.Items[KEY_PERMISSIONS] = accountService.FindByToken(ContextHelper.Token).Permissions;
        }
        catch (Exception e)
        {
            Log.Error(Owner.Will, "Unable to load permissions for a user.", exception: e);
        }
        
        // Prepare the HTTP method for forwarding.
        context.HttpContext.Items[KEY_HTTP_METHOD] = context.HttpContext.Request.Method;

        // Prepare the query parameters for forwarding.
        GenericData param = new GenericData();
        foreach (string key in context.HttpContext.Request.Query.Keys)
        {
            param[key] = context.HttpContext.Request.Query[key];
        }

        if (param.Any())
        {
            context.HttpContext.Items[KEY_PARAMETERS] = param;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}