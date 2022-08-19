using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities;

namespace TowerPortal.Models;

public class FrontendLog : PlatformDataModel
{
    [BsonIgnore, JsonInclude, JsonPropertyName("message"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Message { get; set; }
    
    [BsonIgnore, JsonInclude, JsonPropertyName("data"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GenericData Data { get; set; }
}