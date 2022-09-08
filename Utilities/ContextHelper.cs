using Dmz.Filters;
using Dmz.Models.Permissions;
using Microsoft.AspNetCore.Http;
using Rumble.Platform.Common.Filters;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities;
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable MemberCanBePrivate.Global

namespace Dmz.Utilities;

public static class ContextHelper
{
    public static TokenInfo Token => Fetch<TokenInfo>(PlatformAuthorizationFilter.KEY_TOKEN);
    public static Passport Passport => Fetch<Passport>(PermissionsFilter.KEY_PERMISSIONS);
    public static GenericData Parameters => Fetch<GenericData>(PermissionsFilter.KEY_PARAMETERS);
    public static GenericData Payload => Fetch<GenericData>(PlatformResourceFilter.KEY_BODY);
    public static string HttpMethod => Fetch<string>(PermissionsFilter.KEY_HTTP_METHOD);

    public static bool Fetch(out TokenInfo token) => (token = Token) != default;
    public static bool Fetch(out Passport passport) => (passport = Passport) != default;
    public static bool Fetch(out GenericData parameters) => (parameters = Parameters) != default;

    private static T Fetch<T>(string key)
    {
        IHttpContextAccessor accessor = new HttpContextAccessor();

        if (accessor.HttpContext == null || !accessor.HttpContext.Items.TryGetValue(key, out object value))
        {
            return (T)default;
        }

        return (T)value;
    }
}