using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities.JsonTools;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeAttributes

namespace Dmz.Models.Token;

[BsonIgnoreExtraElements]
public class TokenLog : PlatformCollectionDocument
{
    internal const string DB_KEY_ACTOR = "actor";
    internal const string DB_KEY_ACTION = "action";
    internal const string DB_KEY_UNBAN_TIME = "unbanTime";
    internal const string DB_KEY_TIMESTAMP = "ts";
    internal const string DB_KEY_TARGET = "tgt";
    internal const string DB_KEY_NOTE = "note";

    public const string FRIENDLY_KEY_ACTOR = "actor";
    public const string FRIENDLY_KEY_ACTION = "action";
    public const string FRIENDLY_KEY_UNBAN_TIME = "unbanTime";
    public const string FRIENDLY_KEY_TIMESTAMP = "timestamp";
    public const string FRIENDLY_KEY_TARGET = "target";
    public const string FRIENDLY_KEY_NOTE = "note";
    
    [BsonElement(DB_KEY_ACTOR)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ACTOR)]
    public string Actor { get; private set; }
    
    [BsonElement(DB_KEY_ACTION)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ACTION)]
    public string Action { get; private set; }
    
    [BsonElement(DB_KEY_UNBAN_TIME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_UNBAN_TIME)]
    public long? UnbanTime { get; private set; }
    
    [BsonElement(DB_KEY_TIMESTAMP)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_TIMESTAMP)]
    public long Timestamp { get; private set; }
    
    [BsonElement(DB_KEY_TARGET)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_TARGET)]
    public string Target { get; private set; }
    
    [BsonElement(DB_KEY_NOTE)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_NOTE)]
    public string Note { get; private set; }

    public TokenLog(string actor, string action, long? unbanTime, string target, string note)
    {
        Actor = actor;
        Action = action;
        UnbanTime = unbanTime;
        Timestamp = Rumble.Platform.Common.Utilities.Timestamp.Now;
        Target = target;
        Note = note;
    }
}