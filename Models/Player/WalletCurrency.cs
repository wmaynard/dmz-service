using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;

namespace TowerPortal.Models.Player;

[BsonIgnoreExtraElements]
public class WalletCurrency : PlatformDataModel
{
    internal const string DB_KEY_CURRENCY_ID = "cid";
    internal const string DB_KEY_AMOUNT = "amt";

    public const string FRIENDLY_KEY_CURRENCY_ID = "currencyId";
    public const string FRIENDLY_KEY_AMOUNT = "amount";
    
    [BsonElement(DB_KEY_CURRENCY_ID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_CURRENCY_ID)]
    public string CurrencyId { get; set; }
    
    [BsonElement(DB_KEY_AMOUNT)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_AMOUNT)]
    public long Amount { get; set; }

    public WalletCurrency(string currencyId, long amount)
    {
        CurrencyId = currencyId;
        Amount = amount;
    }
}