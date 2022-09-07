using MongoDB.Bson.Serialization.Attributes;

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class MultiplayerPermissions : PermissionGroup
{
    // ReSharper disable once ArrangeAccessorOwnerBody
    public override string Name => "Multiplayer";
}