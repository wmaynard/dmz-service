using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

[BsonIgnoreExtraElements]
public class Attachment : PlatformDataModel
{
    internal const string DB_KEY_TYPE = "type";
    internal const string DB_KEY_REWARD_ID = "rwdId";
    internal const string DB_KEY_QUANTITY = "qnty";

    public const string FRIENDLY_KEY_TYPE = "type";
    public const string FRIENDLY_KEY_REWARD_ID = "rewardId";
    public const string FRIENDLY_KEY_QUANTITY = "quantity";
        
    [BsonElement(DB_KEY_TYPE)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_TYPE)]
    public string Type { get; private set; }
        
    [BsonElement(DB_KEY_REWARD_ID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_REWARD_ID)]
    public string RewardId { get; private set; }
        
    [BsonElement(DB_KEY_QUANTITY)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_QUANTITY)]
    public int Quantity { get; private set; }
    
    public Attachment(string type, string rewardId, int quantity = 1)
    {
        Type = type;
        RewardId = rewardId;
        Quantity = quantity;
        if (quantity == 0)
        {
            Quantity = 1;
        }
    }
}