using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

public class DetailsResponse : PlatformDataModel
{
    internal const string DB_KEY_PLAYER = "player";
    internal const string DB_KEY_PROFILES = "profiles";
    internal const string DB_KEY_ITEMS = "items";

    public const string FRIENDLY_KEY_PLAYER = "player";
    public const string FRIENDLY_KEY_PROFILES = "profiles";
    public const string FRIENDLY_KEY_ITEMS = "items";
    
    [BsonElement(DB_KEY_PLAYER)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_PLAYER)]
    public DetailsPlayer Player { get; set; }
    
    [BsonElement(DB_KEY_PROFILES)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_PROFILES)]
    public object Profiles { get; set; }
    
    [BsonElement(DB_KEY_ITEMS)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ITEMS)]
    public object Items { get; set; }

    public DetailsResponse(DetailsPlayer player, object profiles, object items)
    {
        Player = player;
        Profiles = profiles;
        Items = items;
    }
}