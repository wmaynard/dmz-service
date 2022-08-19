using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class LeaderboardsPermissions : PermissionGroup
{
    public override string Name => "Leaderboards";
}