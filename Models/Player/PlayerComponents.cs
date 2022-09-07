using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities;

namespace Dmz.Models.Player;

[BsonIgnoreExtraElements]
public class PlayerComponents : PlatformDataModel
{
    internal const string DB_KEY_AB_TEST = "abTest";
    internal const string DB_KEY_ACCOUNT = "account";
    internal const string DB_KEY_EQUIPMENT = "equipment";
    internal const string DB_KEY_HERO = "hero";
    internal const string DB_KEY_MULTIPLAYER = "multiplayer";
    internal const string DB_KEY_QUEST = "quest";
    internal const string DB_KEY_STORE = "store";
    internal const string DB_KEY_SUMMARY = "summary";
    internal const string DB_KEY_TUTORIAL = "tutorial";
    internal const string DB_KEY_WALLET = "wallet";
    internal const string DB_KEY_WORLD = "world";
    
    public const string FRIENDLY_KEY_AB_TEST = "abTest";
    public const string FRIENDLY_KEY_ACCOUNT = "account";
    public const string FRIENDLY_KEY_EQUIPMENT = "equipment";
    public const string FRIENDLY_KEY_HERO = "hero";
    public const string FRIENDLY_KEY_MULTIPLAYER = "multiplayer";
    public const string FRIENDLY_KEY_QUEST = "quest";
    public const string FRIENDLY_KEY_STORE = "store";
    public const string FRIENDLY_KEY_SUMMARY = "summary";
    public const string FRIENDLY_KEY_TUTORIAL = "tutorial";
    public const string FRIENDLY_KEY_WALLET = "wallet";
    public const string FRIENDLY_KEY_WORLD = "world";
    
    [BsonElement(DB_KEY_AB_TEST)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_AB_TEST)]
    public GenericData AbTest { get; set; }
    
    [BsonElement(DB_KEY_ACCOUNT)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ACCOUNT)]
    public GenericData Account { get; set; }
    
    [BsonElement(DB_KEY_EQUIPMENT)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_EQUIPMENT)]
    public GenericData Equipment { get; set; }
    
    [BsonElement(DB_KEY_HERO)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_HERO)]
    public GenericData Hero { get; set; }
    
    [BsonElement(DB_KEY_MULTIPLAYER)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_MULTIPLAYER)]
    public GenericData Multiplayer { get; set; }
    
    [BsonElement(DB_KEY_QUEST)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_QUEST)]
    public GenericData Quest { get; set; }
    
    [BsonElement(DB_KEY_STORE)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_STORE)]
    public GenericData Store { get; set; }
    
    [BsonElement(DB_KEY_SUMMARY)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_SUMMARY)]
    public GenericData Summary { get; set; }
    
    [BsonElement(DB_KEY_TUTORIAL)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_TUTORIAL)]
    public GenericData Tutorial { get; set; }
    
    [BsonElement(DB_KEY_WALLET)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_WALLET)]
    public PlayerWallet Wallet { get; set; }
    
    [BsonElement(DB_KEY_WORLD)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_WORLD)]
    public GenericData World { get; set; }
}