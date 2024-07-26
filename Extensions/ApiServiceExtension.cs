using System;
using Dmz.Exceptions;
using Dmz.Utilities;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Utilities.JsonTools;

namespace Dmz.Extensions;

public static class ApiServiceExtension
{
    /// <summary>
    /// Forwards a request to other Platform services.  If parameters or payload are not specified, the current parameters and payload are used.
    /// </summary>
    /// <param name="apiService">The ApiService singleton.</param>
    /// <param name="url">The url to forward the request to.</param>
    /// <param name="parameters">The query string, translated to a RumbleJson object.</param>
    /// <param name="asAdmin">If set to true, DMZ will attach its own admin token to the request for authorization.</param>
    /// <param name="payload">The request body, translated to a RumbleJson object.  This is accessible in controllers via the Body property.</param>
    /// <returns>RumbleJson representing the request response.</returns>
    /// <exception cref="PlatformException"></exception>
    public static RumbleJson Forward(this ApiService apiService, string url, RumbleJson parameters = null, bool asAdmin = false, RumbleJson payload = null)
    {
        ApiRequest request = apiService.Request(url.TrimStart('/'));

        if (asAdmin)
        {
            string adminToken = DynamicConfig.Instance?.AdminToken;
            if (adminToken != null)
                request.AddAuthorization(adminToken);
        }
        else if (ContextHelper.Fetch(out TokenInfo token))
            request.AddAuthorization(token.Authorization);

        // This will be overwritten if another parameter exists with the key of 'origin'.
        // This origin is helpful for end services finding out where the request came from.
        request.AddParameter("origin", PlatformEnvironment.ServiceName);
        if (parameters != null || ContextHelper.Fetch(out parameters))
            request.AddParameters(parameters);

        if ((payload ??= ContextHelper.Payload) != null)
            request.SetPayload(payload);

        request.OnFailure(_ => Log.Local(Owner.Will, "Failed to get 2xx response.", emphasis: Log.LogType.ERROR));

        RumbleJson json = null;
        int code = -1;
        switch (ContextHelper.HttpMethod)
        {
            case "DELETE":
                request.Delete(out json, out code);
                break;
            case "GET":
                request.Get(out json, out code);
                break;
            case "HEAD":
                request.Head(out json, out code);
                break;
            case "OPTIONS":
                request.Options(out json, out code);
                break;
            case "PATCH":
                request.Patch(out json, out code);
                break;
            case "POST":
                request.Post(out json, out code);
                break;
            case "PUT":
                request.Put(out json, out code);
                break;
            case "TRACE":
                request.Trace(out json, out code);
                break;
            default:
                throw new PlatformException("Unknown HTTP method.");
        }

        return code.Between(200, 299)
            ? json
            : throw new ForwardingException(request.Url, code, json.Optional<RumbleJson>(key: "platformData") ?? json);
    }
    
    public static void ForceRefresh(this ApiService apiService, string aid)
    {
        ApiRequest request = apiService.Request(PlatformEnvironment.Url("/token/admin/invalidate"));
        
        if (ContextHelper.Fetch(out TokenInfo token))
            request.AddAuthorization(token.Authorization);

        request
            .SetPayload(new RumbleJson
            {
                { TokenInfo.FRIENDLY_KEY_ACCOUNT_ID, aid }
            })
            .OnSuccess(_ => Log.Info(Owner.Will, "Invalidating token to force user refresh due to a portal request."))
            .OnFailure(_ => Log.Error(Owner.Will, "Failed to invalidate token when attempting to force user refresh due to a portal request."))
           .Patch();
    }
}