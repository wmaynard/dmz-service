using Dmz.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Filters;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;

namespace Dmz.Filters;

public class ForwardingExceptionFilter : PlatformFilter, IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context?.Exception is not ForwardingException fex)
            return;
        
        // TODO: This is a kluge to prevent alert dialogs from popping on the Titans website.
        // The website currently pops a dialog anytime the response isn't a 2xx, which is an antipattern.
        // DMZ is supposed to return what the end services do, and non-2xx HTTP codes serve their own purposes.
        context.Result = context.GetEndpoint().EndsWith("login")
            ? new OkObjectResult(fex.Data)
            : new BadRequestObjectResult(fex.Data);
        
        context.ExceptionHandled = true;
    }
}