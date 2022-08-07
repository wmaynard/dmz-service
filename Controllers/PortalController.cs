using Rumble.Platform.Common.Web;
using TowerPortal.Extensions;
using TowerPortal.Models;
using TowerPortal.Models.Permissions;

namespace TowerPortal.Controllers;

public abstract class PortalController : PlatformController
{
  protected Passport UserPermissions => ViewData.GetPermissions();
}