using Rumble.Platform.Common.Web;
using TowerPortal.Extensions;
using TowerPortal.Models;

namespace TowerPortal.Controllers;

public abstract class PortalController : PlatformController
{
	public Permissions UserPermissions => ViewData.GetPermissions();
}