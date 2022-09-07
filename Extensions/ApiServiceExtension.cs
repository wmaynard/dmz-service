using Dmz.Utilities;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;

namespace Dmz.Extensions;

public static class ApiServiceExtension
{
    /// <summary>
    /// Forwards a request to other Platform services.  If parameters or payload are not specified, the current parameters and payload are used.
    /// </summary>
    /// <param name="apiService">The ApiService singleton.</param>
    /// <param name="url">The url to forward the request to.</param>
    /// <param name="parameters">The query string, translated to a GenericData object.</param>
    /// <param name="payload">The request body, translated to a GenericData object.  This is accessible in controllers via the Body property.</param>
    /// <returns>GenericData representing the request response.</returns>
    /// <exception cref="PlatformException"></exception>
    public static GenericData Forward(this ApiService apiService, string url, GenericData parameters = null, GenericData payload = null)
    {
        ApiRequest request = apiService.Request(url);

        if (ContextHelper.Fetch(out TokenInfo token))
            request.AddAuthorization(token.Authorization);
        
        if (parameters != null || ContextHelper.Fetch(out parameters))
            request.AddParameters(parameters);

        if ((payload ??= ContextHelper.Payload) != null)
            request.SetPayload(payload);

        GenericData json = null;
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
        
        code.ValidateSuccessCode(url, json);
        
        return json;
    }
}