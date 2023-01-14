using System;
using Dmz.Filters;
using Dmz.Models.Permissions;
using Dmz.Models.Portal;
using Dmz.Services;
using Microsoft.AspNetCore.Http;
using RCL.Logging;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Filters;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable MemberCanBePrivate.Global

namespace Dmz.Utilities;

public static class ContextHelper
{
    public static TokenInfo Token => Fetch<TokenInfo>(PlatformAuthorizationFilter.KEY_TOKEN);
    public static Passport Passport => Fetch<Passport>(PermissionsFilter.KEY_PERMISSIONS);
    public static RumbleJson Parameters => Fetch<RumbleJson>(PermissionsFilter.KEY_PARAMETERS);
    public static RumbleJson Payload => Fetch<RumbleJson>(PlatformResourceFilter.KEY_BODY);
    public static string HttpMethod => Fetch<string>(PermissionsFilter.KEY_HTTP_METHOD);

    public static AuditLog AuditLog
    {
        get => Fetch<AuditLog>(AuditFilter.KEY_AUDIT_LOG);
        set => Store(AuditFilter.KEY_AUDIT_LOG, value);
    }

    public static bool Fetch(out TokenInfo token) => (token = Token) != default;
    public static bool Fetch(out Passport passport) => (passport = Passport) != default;
    public static bool Fetch(out RumbleJson parameters) => (parameters = Parameters) != default;

    private static T Fetch<T>(string key)
    {
        IHttpContextAccessor accessor = new HttpContextAccessor();

        if (accessor.HttpContext == null || !accessor.HttpContext.Items.TryGetValue(key, out object value))
            return (T)default;

        return (T)value;
    }

    private static bool Store<T>(string key, T obj)
    {
        try
        {
            IHttpContextAccessor accessor = new HttpContextAccessor();

            if (accessor.HttpContext?.Items == null)
                return false;
            accessor.HttpContext.Items[key] = obj;
            return true;
        }
        catch (Exception e)
        {
            Log.Warn(Owner.Will, "Unable to store item in HTTP context.", data: new
            {
                Object = obj
            }, exception: e);
        }

        return false;
    }
}