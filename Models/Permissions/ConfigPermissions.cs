using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class ConfigPermissions : PermissionGroup
{
    public override string Name => "Config";
    
    public bool Manage { get; set; }
    public bool Delete { get; set; }
}