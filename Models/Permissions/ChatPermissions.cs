using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dmz.Models.Permissions;

[BsonIgnoreExtraElements]
public class ChatPermissions : PermissionGroup
{
    public override string Name => "Chat Service";
    
    public bool SendAnnouncements { get; set; }
    public bool DeleteMessages { get; set; }
    public bool EditMessages { get; set; }
    public bool ViewMessages { get; set; }
    public bool ViewReports { get; set; }
    public bool EditReports { get; set; }
    public bool ViewRooms { get; set; }
    public bool EditRooms { get; set; }
}