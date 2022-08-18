using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class TokenPermissions : PermissionGroup
{
    public override string Name => "Token Service";
}