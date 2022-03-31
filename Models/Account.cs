using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

public class Account : PlatformCollectionDocument
{
    internal const string DB_KEY_NAME = "name";
    internal const string DB_KEY_EMAIL = "email";
    internal const string DB_KEY_ROLES = "roles";

    public const string FRIENDLY_KEY_NAME = "name";
    public const string FRIENDLY_KEY_EMAIL = "email";
    public const string FRIENDLY_KEY_ROLES = "roles";

    [BsonElement(DB_KEY_NAME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_NAME)]
    public string Name { get; private set; }

    [BsonElement(DB_KEY_EMAIL)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_EMAIL)]
    public string Email { get; private set; }
    
    [BsonElement(DB_KEY_ROLES)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ROLES)]
    public List<string> Roles { get; private set; }
    
    // name
    // email
    // roles
}