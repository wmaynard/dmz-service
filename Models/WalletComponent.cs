using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;

namespace TowerPortal.Models;

[BsonIgnoreExtraElements]
public class WalletComponent : PlatformDataModel
{
    internal const string DB_KEY_IS_DIRTY = "dirty";
    internal const string DB_KEY_VERSION = "ver";

    public const string FRIENDLY_KEY_IS_DIRTY = "isDirty";
    public const string FRIENDLY_KEY_VERSION = "version";
    
    [BsonElement(DB_KEY_IS_DIRTY)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_IS_DIRTY)]
    public bool IsDirty { get; set; }
    
    [BsonElement(DB_KEY_VERSION)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_VERSION)]
    public int Version { get; set; }
}