using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

[BsonIgnoreExtraElements]
public class Permissions : PlatformDataModel
{
	// TODO: Permissions should be renamed to be more English-readable
	// e.g. IsAdmin
	// This will make it easier to understand when used in conditional statements
	public bool Admin { get; set; }
	public bool ManagePermissions { get; set; }
	public bool ViewPlayer { get; set; }
	public bool EditPlayer { get; set; }
	public bool ViewMailbox { get; set; }
	public bool EditMailbox { get; set; }
	public bool ViewToken { get; set; }
	public bool EditToken { get; set; }
	public bool ViewConfig { get; set; }
	public bool EditConfig { get; set; }

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
		ViewConfig = true;
	}
}