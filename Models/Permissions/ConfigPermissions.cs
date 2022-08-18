using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class ConfigPermissions : PermissionGroup
{
    public override string Name => "Config";
}