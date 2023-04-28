using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class PortalPermissions : PermissionGroup
{
    public override string Name => "Portal";
    
    public bool SuperUser         { get; set; }
    public bool ManagePermissions { get; set; }
    public bool ViewActivityLogs { get; set; }
    
    public bool ViewBouncedEmails { get; set; }
}