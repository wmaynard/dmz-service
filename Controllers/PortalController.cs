using System.Linq;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Web;
using TowerPortal.Exceptions;
using TowerPortal.Extensions;
using TowerPortal.Models;
using TowerPortal.Models.Permissions;
using TowerPortal.Views.Shared;

namespace TowerPortal.Controllers;

public abstract class PortalController : PlatformController
{
    protected Passport Permissions => (Passport)ViewData[PortalView.KEY_PERMISSIONS] ?? new Passport();

    protected void Require(params bool[] permissions)
    {
        if (permissions.Any(_bool => !_bool)) 
            throw new PermissionInvalidException();
    }
}