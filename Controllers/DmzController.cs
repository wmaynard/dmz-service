using System.Linq;
using Dmz.Exceptions;
using Dmz.Models.Permissions;
using Dmz.Utilities;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Web;
using Dmz.Extensions;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

// ReSharper disable InconsistentNaming

namespace Dmz.Controllers;

public abstract class DmzController : PlatformController
{
    protected static Passport Permissions => ContextHelper.Passport
        ?? throw new PermissionInvalidException();

    protected static bool Require(params bool[] permissions) => permissions.Any(_bool => !_bool)
        ? throw new PermissionInvalidException()
        : true;

    protected static bool RequireOneOf(params bool[] permissions) => permissions.Any(boolean => boolean)
        ? true
        : throw new PermissionInvalidException();

    protected ActionResult Forward(string url) => Ok(data: _apiService.Forward(url));
    
    protected ActionResult Forward(string url, out RumbleJson response)
    {
        response = _apiService.Forward(url);
        return Ok(data: response);
    }
}