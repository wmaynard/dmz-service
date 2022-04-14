using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

public class GlobalMessageResponse : PlatformDataModel
{
    internal const string DB_KEY_SUCCESS = "success";
    internal const string DB_KEY_GLOBAL_MESSAGES = "glblMsg";

    public const string FRIENDLY_KEY_SUCCESS = "success";
    public const string FRIENDLY_KEY_GLOBAL_MESSAGES = "globalMessages";
    
    [BsonElement(DB_KEY_SUCCESS)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_SUCCESS)]
    public bool Success { get; set; }
    
    [BsonElement(DB_KEY_GLOBAL_MESSAGES)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_GLOBAL_MESSAGES)]
    public List<GlobalMessage> GlobalMessages { get; set; }

    public GlobalMessageResponse(bool success, List<GlobalMessage> globalMessages)
    {
        Success = success;
        GlobalMessages = globalMessages;
    }
}