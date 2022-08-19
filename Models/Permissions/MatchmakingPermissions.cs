using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class MatchmakingPermissions : PermissionGroup
{
    public override string Name => "Matchmaking";
}