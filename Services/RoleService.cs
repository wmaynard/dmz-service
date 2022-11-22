using Dmz.Models.Permissions;
using Rumble.Platform.Common.Services;

namespace Dmz.Services;

public class RoleService : PlatformMongoService<Role>
{
	public RoleService() : base(collection: "roles") { }

	public Role FindByName(string name)
	{
		return FindOne(filter: role => role.Name == name);
	}
}