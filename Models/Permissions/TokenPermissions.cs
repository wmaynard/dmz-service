using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class TokenPermissions : PermissionGroup
{
    public override string Name => "Token Service";
    
    public bool Ban        { get; set; }
    public bool Unban      { get; set; }
    public bool Invalidate { get; set; }
}