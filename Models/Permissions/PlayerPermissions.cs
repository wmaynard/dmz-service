using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class PlayerPermissions : PermissionGroup
{
    public override string Name => "Player Service";
    
    public bool Search          { get; set; } // necessary? same use case as view_page?
    public bool Screenname      { get; set; }
    public bool Unlink_Accounts { get; set; }
    public bool Update          { get; set; }
    public bool ForceAccountLink { get; set; }
}