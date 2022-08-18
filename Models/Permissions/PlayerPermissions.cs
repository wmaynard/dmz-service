using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class PlayerPermissions : PermissionGroup
{
    public override string Name => "Player Service";
    
    // TODO: Add permissions
    public bool Search { get; set; }
}