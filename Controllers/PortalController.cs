using Rumble.Platform.Common.Web;
using TowerPortal.Extensions;
using TowerPortal.Models;
using TowerPortal.Models.Permissions;
using TowerPortal.Views.Shared;

namespace TowerPortal.Controllers;

public abstract class PortalController : PlatformController
{
  protected Passport UserPermissions => (Passport)ViewData[PortalView.KEY_PERMISSIONS] ?? new Passport();
}