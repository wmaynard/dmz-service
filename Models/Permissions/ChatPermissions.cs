using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class ChatPermissions : PermissionGroup
{
  public override string Name => "Chat Service";
}