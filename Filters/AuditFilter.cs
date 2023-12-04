using System;
using System.Linq;
using System.Net.Http;
using Dmz.Models.Portal;
using Dmz.Services;
using Dmz.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RCL.Logging;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Filters;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Filters;

public class AuditFilter : PlatformFilter, IActionFilter
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

    public void OnActionExecuted(ActionExecutedContext context)
    {
        AuditLog log = ContextHelper.AuditLog;

        if (log == null)
            return;

        try
        {
            if (AccountService.Instance?.AddLog(log.SetCode(context.HttpContext.Response.StatusCode)) == null)
                Log.Warn(Owner.Will, "Failed to commit activity log.");
        }
        catch (Exception e)
        {
            Log.Error(Owner.Will, "Failed to commit activity log.", exception: e);
        }

    }

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
            PortalAccountId = account.Id,
            Endpoint = _context.Request.Path.Value,
            Message = null,
            Method = _context.Request.Method,
            Who = token.Email ?? token.ScreenName ?? token.AccountId ?? "unknown",
            AdditionalData = null,
            Time = Timestamp.Now
        };
    }

    /// <summary>
    /// Updates the activity log with the specified message.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public static void UpdateLog(string message, RumbleJson data = null)
    {
        AuditLog log = ContextHelper.AuditLog;

        log.Message = message;

        if (log.AdditionalData == null)
            log.AdditionalData = data;
        else if (data != null)
            log.AdditionalData.Combine(data, prioritizeOther: true);

        ContextHelper.AuditLog = log;
    }
}