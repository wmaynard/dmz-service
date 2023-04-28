using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;

namespace Dmz.Exceptions;

public class EmailBannedException : PlatformException
{
    public string Address { get; init; }
    public string Detail => "To appeal this error, contact customer service.";
    
    public EmailBannedException(string address) : base(message: "Email has been banned due to hard bounces or validation errors", code: ErrorCode.NoLongerValid)
    {
        Address = address;
    }
}