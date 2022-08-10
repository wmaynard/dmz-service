using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;

namespace TowerPortal.Models;

[BsonIgnoreExtraElements]
public class ChatBan : PlatformCollectionDocument
{
  internal const string DB_KEY_ID             = "id";
  internal const string DB_KEY_ACCOUNT_ID     = "aid";
  internal const string DB_KEY_ISSUED         = "issued";
  internal const string DB_KEY_REASON         = "reason";
  internal const string DB_KEY_SNAPSHOT       = "snapshot";
  internal const string DB_KEY_TIME_REMAINING = "tmeRmn";

  public const string FRIENDLY_KEY_ID             = "id";
  public const string FRIENDLY_KEY_ACCOUNT_ID     = "aid";
  public const string FRIENDLY_KEY_ISSUED         = "issued";
  public const string FRIENDLY_KEY_REASON         = "reason";
  public const string FRIENDLY_KEY_SNAPSHOT       = "snapshot";
  public const string FRIENDLY_KEY_TIME_REMAINING = "timeRemaining";

  [BsonElement(DB_KEY_ID)]
  [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ID)]
  public new string Id { get; private set; }
    
  [BsonElement(DB_KEY_ACCOUNT_ID)]
  [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ACCOUNT_ID)]
  public string Aid { get; private set; }
    
  [BsonElement(DB_KEY_ISSUED)]
  [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ISSUED)]
  public long Issued { get; private set; }
    
  [BsonElement(DB_KEY_REASON)]
  [JsonInclude, JsonPropertyName(FRIENDLY_KEY_REASON)]
  public string Reason { get; private set; }
    
  [BsonElement(DB_KEY_SNAPSHOT)]
  [JsonInclude, JsonPropertyName(FRIENDLY_KEY_SNAPSHOT)]
  public List<Object> Snapshot { get; private set; }
    
  [BsonElement(DB_KEY_TIME_REMAINING)]
  [JsonInclude, JsonPropertyName(FRIENDLY_KEY_TIME_REMAINING)]
  public string TimeRemaining { get; private set; }
    
  public ChatBan(string id, string aid, long issued, string reason, List<Object> snapshot, string timeRemaining)
  {
    Id = id;
    Aid = aid;
    Issued = issued;
    Reason = reason;
    Snapshot = snapshot;
    TimeRemaining = timeRemaining;
  }
}