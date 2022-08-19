using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class ConfigPermissions : PermissionGroup
{
    public override string Name => "Config";
    
    public bool Manage { get; set; }
    public bool Delete { get; set; }
}