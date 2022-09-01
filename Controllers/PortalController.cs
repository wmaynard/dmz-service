using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Web;
using TowerPortal.Exceptions;
using TowerPortal.Extensions;
using TowerPortal.Models.Permissions;
using TowerPortal.Utilities;

namespace TowerPortal.Controllers;

public abstract class PortalController : PlatformController
{
    protected static Passport Permissions
    {
        get
        {
            return ContextHelper.Passport
                   ?? throw new PermissionInvalidException();
        }
    }

    protected static bool Require(params bool[] permissions)
    {
        return permissions.Any(_bool => !_bool)
                   ? throw new PermissionInvalidException()
                   : true;
    }

    protected static bool RequireOneOf(params bool[] permissions)
    {
        return permissions.Any(boolean => boolean)
                   ? true
                   : throw new PermissionInvalidException();
    }

    protected ActionResult Forward(string url)
    {
        return Ok(data: _apiService.Forward(url));
    }
}