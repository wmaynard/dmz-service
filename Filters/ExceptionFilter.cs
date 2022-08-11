using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RCL.Logging;
using Rumble.Platform.Common.Filters;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Exceptions;
using TowerPortal.Models;

namespace TowerPortal.Filters;

public class ExceptionFilter : PlatformFilter, IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case PermissionInvalidException:
                context.Result = new RedirectResult("/portal/error"); // TODO: Unauthorized page
                break;
            default:
                context.ExceptionHandled = false;
                break;
        }
        if (context.ExceptionHandled)
            Log.Local(Owner.Will, $"Exception caught and handled by a filter: {context.Exception.GetType().Name}");
    }
}