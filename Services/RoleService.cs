using Rumble.Platform.Common.Web;
using TowerPortal.Models;

namespace TowerPortal.Services;

public class RoleService : PlatformMongoService<Role>
{
    public RoleService() : base(collection: "Roles")
    {
        
    }
}