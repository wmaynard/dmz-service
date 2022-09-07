using MongoDB.Bson.Serialization.Attributes;

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class LeaderboardsPermissions : PermissionGroup
{
    // ReSharper disable once ArrangeAccessorOwnerBody
    public override string Name => "Leaderboards";
}