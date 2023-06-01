using Dmz.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rumble.Platform.Common.Filters;

namespace Dmz.Filters;

public class ForwardingExceptionFilter : PlatformFilter, IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ForwardingException fex)
            return;
        
        context.Result = new BadRequestObjectResult(fex.Data);
        context.ExceptionHandled = true;
    }
}