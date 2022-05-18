using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

public class Permissions : PlatformDataModel
{
	public bool Admin { get; set; }
	public bool ViewPlayer { get; set; }
	public bool EditPlayer { get; set; }
	public bool ViewMailbox { get; set; }
	public bool EditMailbox { get; set; }

	public void SetAdmin()
	{
		Admin = true;
	}
}