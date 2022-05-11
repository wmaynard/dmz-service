using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

public class Permissions : PlatformDataModel
{
	public bool ViewPlayer { get; set; }
	public bool EditPlayer { get; set; }
}