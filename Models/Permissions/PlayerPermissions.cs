using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class PlayerPermissions : PermissionGroup
{
    public override string Name => "Player Service";
    
    public bool Search          { get; set; } // necessary? same use case as view_page?
    public bool Screenname      { get; set; }
    public bool Unlink_Accounts { get; set; }
    public bool Add_Currency    { get; set; }
    public bool Remove_Currency { get; set; }
}