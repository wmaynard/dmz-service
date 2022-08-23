using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class MailPermissions : PermissionGroup
{
    public override string Name => "Mail Service";

    public bool Send_Direct_Messages   { get; set; } // TODO: Implement
    public bool Send_Global_Messages   { get; set; } // TODO: Implement
    public bool Delete_Global_Messages { get; set; }
    public bool Modify_Inbox           { get; set; }
}