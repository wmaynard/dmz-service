using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;

namespace TowerPortal.Models;

[BsonIgnoreExtraElements]
public class WalletData : PlatformDataModel
{
    internal const string DB_KEY_CURRENCIES = "currencies";
    internal const string DB_KEY_COMPONENT = "comp";

    public const string FRIENDLY_KEY_CURRENCIES = "currencies";
    public const string FRIENDLY_KEY_COMPONENT = "component";
    
    [BsonElement(DB_KEY_CURRENCIES)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_CURRENCIES)]
    public List<WalletCurrency> Currencies { get; set; }
    
    [BsonElement(DB_KEY_COMPONENT)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_COMPONENT)]
    public WalletComponent Component { get; set; }
}