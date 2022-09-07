using MongoDB.Bson.Serialization.Attributes;

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class ReceiptPermissions : PermissionGroup
{
    // ReSharper disable once ArrangeAccessorOwnerBody
    public override string Name => "Receipt";
}