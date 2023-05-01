using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;

namespace Dmz.Exceptions;

public class EmailNotWhitelistedException : PlatformException
{
    public string Address { get; init; }
    public string Help => "The whitelist can be configured via Portal.";

    public EmailNotWhitelistedException(string email) : base(message: "Email domain has not been whitelisted.", code: ErrorCode.Unauthorized)
    {
        Address = email;
    }
}