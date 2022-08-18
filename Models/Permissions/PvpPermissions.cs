using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class PvpPermissions : PermissionGroup
{
    public override string Name => "PvP";
}