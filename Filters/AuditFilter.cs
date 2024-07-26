using System;
using System.Linq;
using System.Net.Http;
using Dmz.Models.Portal;
using Dmz.Services;
using Dmz.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Filters;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Utilities.JsonTools;

namespace Dmz.Filters;

public class AuditFilter : PlatformFilter, IActionFilter, IResultFilter
{
    public const string KEY_AUDIT_LOG = "AuditLog";
    
    public void OnActionExecuting(ActionExecutingContext context)
    {
        try
        {
            string[] monitored =
            {
                HttpMethod.Delete.ToString(),
                HttpMethod.Patch.ToString(),
                HttpMethod.Post.ToString(),
                HttpMethod.Put.ToString(),
            };
            string method = context.HttpContext.Request.Method;

            if (!monitored.Contains(method))
                return;
        
            CreateLog(context);
        }
        catch (InvalidTokenException) { }
        catch (Exception e)
        {
            Log.Error(Owner.Will, "Failed to create activity log.", exception: e);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }

    private static void CreateLog(ActionContext context = null)
    {
        HttpContext _context = context?.HttpContext ?? new HttpContextAccessor().HttpContext;
        TokenInfo token = ContextHelper.Token;

        if (context == null || token == null || token.IsNotAdmin)
            return;
        
        Account account = AccountService.Instance?.FindByToken(token);

        if (account == null)
            return;
        
        ContextHelper.AuditLog = new AuditLog
        {
            Endpoint = _context.Request.Path.Value,
            Message = null,
            Method = _context.Request.Method,
            Who = token,
            Request = ContextHelper.Payload
        };
    }

    /// <summary>
    /// Updates the activity log with the specified message.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="additionalData"></param>
    public static void UpdateLog(string message, RumbleJson additionalData = null)
    {
        AuditLog log = ContextHelper.AuditLog;

        log.Message = message;

        if (log.AdditionalData == null)
            log.AdditionalData = additionalData;
        else if (additionalData != null)
            log.AdditionalData.Combine(additionalData, prioritizeOther: true);

        ContextHelper.AuditLog = log;
    }

    public void OnResultExecuting(ResultExecutingContext context) { }

    public void OnResultExecuted(ResultExecutedContext context) => CaptureLog(context);

    public static void CaptureLog(FilterContext context, int? statusCode = null)
    {
        AuditLog log = ContextHelper.AuditLog;

        if (log == null)
            return;
        
        int code = statusCode ?? context.HttpContext.Response.StatusCode;
        IActionResult result = context switch
        {
            _ when code.Between(200, 299) => null,
            ExceptionContext ex => ex.Result,
            ResultExecutedContext res => res.Result,
            _ => null
        };
        if (result is ObjectResult { Value: RumbleJson json })
            log.Response = json;
        
        PlatformService.Require<ActivityLogService>().Insert(log.SetCode(code));
    }
}