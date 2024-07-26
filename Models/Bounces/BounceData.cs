using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities.JsonTools;

namespace Dmz.Models.Bounces;

public class BounceData : PlatformCollectionDocument
{
    internal const string DB_KEY_EMAIL = "email";
    internal const string DB_KEY_HARD_BOUNCES = "hard";
    internal const string DB_KEY_SOFT_BOUNCES = "soft";
    internal const string DB_KEY_BANNED = "ban";
    internal const string DB_KEY_LAST_BOUNCE = "ts";
    
    public const string FRIENDLY_KEY_EMAIL = "email";
    public const string FRIENDLY_KEY_HARD_BOUNCES = "hard";
    public const string FRIENDLY_KEY_SOFT_BOUNCES = "soft";
    public const string FRIENDLY_KEY_BANNED = "ban";
    public const string FRIENDLY_KEY_LAST_BOUNCE = "ts";
    
    [BsonElement(DB_KEY_EMAIL)]
    [JsonPropertyName(FRIENDLY_KEY_EMAIL)]
    [SimpleIndex(unique: true)]
    public string Email { get; set; }
    
    [BsonElement(DB_KEY_BANNED)]
    [JsonPropertyName(FRIENDLY_KEY_BANNED)]
    public bool Banned { get; set; }
    
    [BsonElement(DB_KEY_HARD_BOUNCES)]
    [JsonPropertyName(FRIENDLY_KEY_HARD_BOUNCES)]
    public long LifetimeHardBounces { get; set; }
    
    [BsonElement(DB_KEY_SOFT_BOUNCES)]
    [JsonPropertyName(FRIENDLY_KEY_SOFT_BOUNCES)]
    public long LifetimeSoftBounces { get; set; }
    
    [BsonElement(DB_KEY_LAST_BOUNCE)]
    [JsonPropertyName(FRIENDLY_KEY_LAST_BOUNCE)]
    public long LastBounce { get; set; }
}