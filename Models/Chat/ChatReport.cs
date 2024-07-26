using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Utilities.JsonTools;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeAttributes

namespace Dmz.Models.Chat;

[BsonIgnoreExtraElements]
public class ChatReport : PlatformDataModel
{
    internal const string DB_KEY_ID         = "id";
    internal const string DB_KEY_MESSAGE_ID = "msgid";
    internal const string DB_KEY_LOG        = "log";
    internal const string DB_KEY_PLAYERS    = "players";
    internal const string DB_KEY_REPORTED   = "reported";
    internal const string DB_KEY_REPORTERS    = "reporters";
    internal const string DB_KEY_STATUS   = "status";
    internal const string DB_KEY_TIME       = "time";

    public const string FRIENDLY_KEY_ID         = "id";
    public const string FRIENDLY_KEY_MESSAGE_ID = "messageId";
    public const string FRIENDLY_KEY_LOG        = "log";
    public const string FRIENDLY_KEY_PLAYERS    = "players";
    public const string FRIENDLY_KEY_REPORTED   = "reported";
    public const string FRIENDLY_KEY_REPORTERS    = "reporters";
    public const string FRIENDLY_KEY_STATUS   = "status";
    public const string FRIENDLY_KEY_TIME       = "time";

    [BsonElement(DB_KEY_ID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ID)]
    public string Id { get; private set; }
      
    [BsonElement(DB_KEY_MESSAGE_ID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_MESSAGE_ID)]
    public string MessageId { get; private set; }
      
    [BsonElement(DB_KEY_LOG)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_LOG)]
    public List<RumbleJson> Log { get; private set; }
      
    [BsonElement(DB_KEY_PLAYERS)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_PLAYERS)]
    public List<RumbleJson> Players { get; private set; }
      
    [BsonElement(DB_KEY_REPORTED)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_REPORTED)]
    public RumbleJson Reported { get; private set; }
    
    [BsonElement(DB_KEY_REPORTERS)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_REPORTERS)]
    public List<RumbleJson> Reporters { get; private set; }
      
    [BsonElement(DB_KEY_STATUS)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_STATUS)]
    public string Status { get; private set; }
      
    [BsonElement(DB_KEY_TIME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_TIME)]
    public long Time { get; private set; }
      
    public ChatReport(string id, string messageId, List<RumbleJson> log, List<RumbleJson> players, RumbleJson reported, List<RumbleJson> reporters, string status, long time)
    {
        Id = id;
        MessageId = messageId;
        Log = log;
        Players = players;
        Reported = reported;
        Reporters = reporters;
        Status = status;
        Time = time;
    }
}