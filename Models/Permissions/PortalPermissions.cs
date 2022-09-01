using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class PortalPermissions : PermissionGroup
{
    public override string Name => "Portal";
    
    public bool SuperUser         { get; set; }
    public bool ManagePermissions { get; set; }
}