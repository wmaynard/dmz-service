using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

public class SearchResult : PlatformDataModel
{
    internal const string DB_KEY_PLAYER = "player";
    internal const string DB_KEY_SCORE = "score";
    internal const string DB_KEY_CONFIDENCE = "conf";

    public const string FRIENDLY_KEY_PLAYER = "player";
    public const string FRIENDLY_KEY_SCORE = "score";
    public const string FRIENDLY_KEY_CONFIDENCE = "confidence";
    
    [BsonElement(DB_KEY_PLAYER)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_PLAYER)]
    public SearchPlayer Player { get; set; }
    
    [BsonElement(DB_KEY_SCORE)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_SCORE)]
    public decimal Score { get; set; }
    
    [BsonElement(DB_KEY_CONFIDENCE)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_CONFIDENCE)]
    public decimal Confidence { get; set; }

    public SearchResult(SearchPlayer player, decimal score, decimal confidence)
    {
        Player = player;
        Score = score;
        Confidence = confidence;
    }
}