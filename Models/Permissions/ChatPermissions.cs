using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class ChatPermissions : PermissionGroup
{
    public override string Name => "Chat Service";
  
    public bool Send_Announcements   { get; set; }
    public bool Delete_Announcements { get; set; }
    public bool Ban                  { get; set; }
    public bool Unban                { get; set; }
    public bool Ignore_Reports       { get; set; }
    public bool Delete_Reports       { get; set; }
}