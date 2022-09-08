using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class MultiplayerPermissions : PermissionGroup
{
    public override string Name => "Multiplayer";
}