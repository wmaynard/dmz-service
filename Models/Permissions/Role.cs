using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Data;

namespace Dmz.Models.Permissions;

public class Role : PlatformCollectionDocument
{
	internal const string DB_KEY_NAME        = "name";
	internal const string DB_KEY_PERMISSIONS = "perms";

	public const string FRIENDLY_KEY_NAME        = "name";
	public const string FRIENDLY_KEY_PERMISSIONS = "permissions";

	[BsonElement(DB_KEY_NAME)]
	[JsonInclude, JsonPropertyName(FRIENDLY_KEY_NAME)]
	public string Name { get; set; }
	
	[BsonElement(DB_KEY_PERMISSIONS)]
	[JsonInclude, JsonPropertyName(FRIENDLY_KEY_PERMISSIONS)]
	public Passport Permissions { get; set; }
	
	public Role(string name, Passport passport)
	{
		Name = name;
		Permissions = passport;
	}
}