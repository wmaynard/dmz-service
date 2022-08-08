using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;

namespace TowerPortal.Models;

[BsonIgnoreExtraElements]
public class Announcement : PlatformCollectionDocument
{
    internal const string DB_KEY_ID = "id";
    internal const string DB_KEY_AID = "aid";
    internal const string DB_KEY_TIMESTAMP = "tmestmp";
    internal const string DB_KEY_EXPIRATION = "expire";
    internal const string DB_KEY_TEXT = "text";
    internal const string DB_KEY_TYPE = "type";

    public const string FRIENDLY_KEY_ID = "id";
    public const string FRIENDLY_KEY_AID = "aid";
    public const string FRIENDLY_KEY_TIMESTAMP = "timestamp";
    public const string FRIENDLY_KEY_EXPIRATION = "expiration";
    public const string FRIENDLY_KEY_TEXT = "text";
    public const string FRIENDLY_KEY_TYPE = "type";

    [BsonElement(DB_KEY_ID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ID)]
    public new string Id { get; private set; }
    
    [BsonElement(DB_KEY_AID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_AID)]
    public string Aid { get; private set; }
    
    [BsonElement(DB_KEY_TIMESTAMP)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_TIMESTAMP)]
    public long Timestamp { get; private set; }
    
    [BsonElement(DB_KEY_EXPIRATION)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_EXPIRATION)]
    public long Expiration { get; private set; }
    
    [BsonElement(DB_KEY_TEXT)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_TEXT)]
    public string Text { get; private set; }
    
    [BsonElement(DB_KEY_TYPE)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_TYPE)]
    public string Type { get; private set; }
    
    public Announcement(string id, string aid, long timestamp, long expiration, string text, string type)
    {
        Id = id;
        Aid = aid;
        Timestamp = timestamp;
        Expiration = expiration;
        Text = text;
        Type = type;
    }
}