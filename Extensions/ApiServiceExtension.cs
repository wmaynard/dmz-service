using Dmz.Utilities;
using RCL.Logging;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

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
        ApiRequest request = apiService.Request(PlatformEnvironment.Url(url.TrimStart('/')));

        if (asAdmin)
        {
            string adminToken = DynamicConfig.Instance?.AdminToken;
            if (adminToken != null)
                request.AddAuthorization(adminToken);
        }
        else if (ContextHelper.Fetch(out TokenInfo token))
            request.AddAuthorization(token.Authorization);
        
        if (parameters != null || ContextHelper.Fetch(out parameters))
            request.AddParameters(parameters);

        if ((payload ??= ContextHelper.Payload) != null)
            request.SetPayload(payload);

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

        if (!code.Between(200, 299))
        {
            RumbleJson errorJson = json.Optional<RumbleJson>(key: "platformData");
            Log.Error(owner: Owner.Nathan, message: "DMZ did not receive a 2xx response from a service.", data: errorJson ?? json);
        }
        
        return json;
    }
    
    public static void ForceRefresh(this ApiService apiService, string aid)
    {
        ApiRequest request = apiService.Request(PlatformEnvironment.Url("/token/admin/invalidate"));
        
        if (ContextHelper.Fetch(out TokenInfo token))
            request.AddAuthorization(token.Authorization);

        request
            .SetPayload(new RumbleJson
            {
                { "aid", aid }
            })
            .OnSuccess(response =>
            {
                Log.Info(owner: Owner.Nathan, message: "Invalidating token to force user refresh due to a portal request.");
            })
            .OnFailure(response =>
            {
                Log.Error(owner: Owner.Nathan, message: "Failed to invalidate token when attempting to force user refresh due to a portal request.");
            })
           .Patch();
    }
}