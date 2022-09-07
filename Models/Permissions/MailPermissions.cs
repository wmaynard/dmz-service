using MongoDB.Bson.Serialization.Attributes;

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class MailPermissions : PermissionGroup
{
    // ReSharper disable once ArrangeAccessorOwnerBody
    public override string Name => "Mail Service";

    public bool Send_Direct_Messages   { get; set; } // TODO: Implement
    public bool Send_Global_Messages   { get; set; } // TODO: Implement
    public bool Expire_Global_Messages { get; set; }
    public bool Modify_Inbox           { get; set; }
}