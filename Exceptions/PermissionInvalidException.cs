using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;

namespace Dmz.Exceptions;

// TODO: Will to grab attempted action via reflection, or some other method to provide information as necessary to make this more helpful.
public class PermissionInvalidException : PlatformException
{
    public PermissionInvalidException() : base(message: "User does not have valid permissions to perform an action.", code: ErrorCode.Unauthorized){ }
}