using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class ReceiptPermissions : PermissionGroup
{
    public override string Name => "Receipt";
}