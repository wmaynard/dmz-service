using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Data;

namespace Dmz.Exceptions;

public class ForwardingException : PlatformException
{
    public RumbleJson Data { get; set; }
    public int HttpCode { get; set; }
    public string Url { get; set; }

    public ForwardingException(string url, int httpCode, RumbleJson data) : base("DMZ did not receive a 2xx response from a service.", code: ErrorCode.ApiFailure)
    {
        Url = url;
        HttpCode = httpCode;
        Data = data;
    }
}