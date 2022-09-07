using MongoDB.Bson.Serialization.Attributes;

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class PlayerPermissions : PermissionGroup
{
    // ReSharper disable once ArrangeAccessorOwnerBody
    public override string Name => "Player Service";
    
    public bool Search          { get; set; } // necessary? same use case as view_page?
    public bool Screenname      { get; set; }
    public bool Unlink_Accounts { get; set; }
    public bool Update          { get; set; }
}