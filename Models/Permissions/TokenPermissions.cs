using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class TokenPermissions : PermissionGroup
{
    public override string Name => "Token Service";
    
    public bool Ban        { get; set; }
    public bool Unban      { get; set; }
    public bool Invalidate { get; set; }
}