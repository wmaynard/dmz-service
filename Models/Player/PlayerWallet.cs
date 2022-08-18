using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;

namespace TowerPortal.Models.Player;

[BsonIgnoreExtraElements]
public class PlayerWallet : PlatformDataModel
{
    internal const string DB_KEY_AID = "aid";
    internal const string DB_KEY_DATA = "data";
    internal const string DB_KEY_NAME = "name";
    internal const string DB_KEY_VERSION = "version";
    internal const string DB_KEY_ID = "id";

    public const string FRIENDLY_KEY_AID = "aid";
    public const string FRIENDLY_KEY_DATA = "data";
    public const string FRIENDLY_KEY_NAME = "name";
    public const string FRIENDLY_KEY_VERSION = "version";
    public const string FRIENDLY_KEY_ID = "id";
    
    [BsonElement(DB_KEY_AID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_AID)]
    public string Aid { get; set; }
    
    [BsonElement(DB_KEY_DATA)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_DATA)]
    public WalletData Data { get; set; }
    
    [BsonElement(DB_KEY_NAME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_NAME)]
    public string Name { get; set; }
    
    [BsonElement(DB_KEY_VERSION)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_VERSION)]
    public int Version { get; set; }
    
    [BsonElement(DB_KEY_ID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ID)]
    public string Id { get; set; }
}