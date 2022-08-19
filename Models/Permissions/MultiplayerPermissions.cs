using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class MultiplayerPermissions : PermissionGroup
{
    public override string Name => "Multiplayer";
}