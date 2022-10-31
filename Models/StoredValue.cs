using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Models;

public class StoredValue : PlatformCollectionDocument
{
    public StoredValue() => CreatedOn = Timestamp.UnixTime;
    
    [BsonElement("v")]
    [JsonPropertyName("value")]
    public string Value { get; set; }
    
    [BsonElement("aid")]
    [JsonIgnore]
    public string AccountId { get; set; }
    
    [BsonElement("ts")]
    public long CreatedOn { get; set; }
    
    [BsonElement("used")]
    public bool Claimed { get; set; }
}