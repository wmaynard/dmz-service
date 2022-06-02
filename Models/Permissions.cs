using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

public class Permissions : PlatformDataModel
{
	public bool Admin { get; set; }
	public bool ManagePermissions { get; set; }
	public bool ViewPlayer { get; set; }
	public bool EditPlayer { get; set; }
	public bool ViewMailbox { get; set; }
	public bool EditMailbox { get; set; }
	public bool ViewToken { get; set; }
	public bool EditToken { get; set; }

	public void SetAdmin()
	{
		Admin = true;
		ManagePermissions = true;
	}

	public void SetUser()
	{
		ViewPlayer = true;
		ViewMailbox = true;
		ViewToken = true;
	}
}