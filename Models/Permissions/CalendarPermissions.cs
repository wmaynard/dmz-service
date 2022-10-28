using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable ArrangeAccessorOwnerBody

namespace Dmz.Models.Permissions;


[BsonIgnoreExtraElements]
public class CalendarPermissions : PermissionGroup
{
	public override string Name => "Calendar Service";
}