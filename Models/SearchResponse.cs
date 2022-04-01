using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

[BsonIgnoreExtraElements]
public class SearchResponse : PlatformDataModel
{
    internal const string DB_KEY_SUCCESS = "success";
    internal const string DB_KEY_RESULTS = "results";

    public const string FRIENDLY_KEY_SUCCESS = "success";
    public const string FRIENDLY_KEY_RESULTS = "results";
    
    [BsonElement(DB_KEY_SUCCESS)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_SUCCESS)]
    public bool Success { get; private set; }
    
    [BsonElement(DB_KEY_RESULTS)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_RESULTS)]
    public List<SearchResult> Results { get; private set; }

    public SearchResponse(bool success, List<SearchResult> searchResults)
    {
        Success = success;
        Results = searchResults;
    }
}