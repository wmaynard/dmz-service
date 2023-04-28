using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Data;

namespace Dmz.Models.Bounces;

public class BounceDataPoint : PlatformCollectionDocument
{
    [BsonElement(BounceData.DB_KEY_EMAIL)]
    [JsonPropertyName(BounceData.FRIENDLY_KEY_EMAIL)]
    public string Email { get; set; }
    
    [BsonElement("code")]
    [JsonPropertyName("errorCode")]
    public int Code { get; set; }
    
    [BsonElement("detail")]
    [JsonPropertyName("detail")]
    public string Detail { get; set; }
    
    [BsonElement("ts")]
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
    
    [BsonElement("tsp")]
    [JsonPropertyName("processedOn")]
    public long TimeProcessed { get; set; }
}