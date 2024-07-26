using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Utilities.JsonTools;

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
	[JsonIgnore]
	public Passport Permissions { get; set; }
	
	[BsonIgnore]
	[JsonInclude, JsonPropertyName(FRIENDLY_KEY_PERMISSIONS)]
	public Dictionary<string, bool> Perms => Permissions
		.SelectMany(group => group.Values)
		.ToDictionary(
			keySelector: pair => pair.Key,
			elementSelector: pair => pair.Value
		);
}