using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;

namespace TowerPortal.Models;

[BsonIgnoreExtraElements]
public class StickyMessage : PlatformDataModel
{
  internal const string DB_KEY_TEXT                = "txt";
  internal const string DB_KEY_DURATION_IN_SECONDS = "durSec";
  internal const string DB_KEY_EXPIRATION          = "exp";
  internal const string DB_KEY_VISIBLE_FROM        = "vis";
  internal const string DB_KEY_LANGUAGE            = "lang";

  public const string FRIENDLY_KEY_TEXT                = "text";
  public const string FRIENDLY_KEY_DURATION_IN_SECONDS = "durationInSeconds";
  public const string FRIENDLY_KEY_EXPIRATION          = "expiration";
  public const string FRIENDLY_KEY_VISIBLE_FROM        = "visibleFrom";
  public const string FRIENDLY_KEY_LANGUAGE            = "language";
  
  [BsonElement(DB_KEY_TEXT)]
  [JsonInclude, JsonPropertyName(FRIENDLY_KEY_TEXT)]
  public string Text { get; private set; }
  
  [BsonElement(DB_KEY_DURATION_IN_SECONDS), BsonIgnoreIfNull]
  [JsonInclude, JsonPropertyName(FRIENDLY_KEY_DURATION_IN_SECONDS)]
  public long? DurationInSeconds { get; private set; }
  
  [BsonElement(DB_KEY_EXPIRATION), BsonIgnoreIfNull]
  [JsonInclude, JsonPropertyName(FRIENDLY_KEY_EXPIRATION)]
  public long? Expiration { get; private set; }
  
  [BsonElement(DB_KEY_VISIBLE_FROM), BsonIgnoreIfNull]
  [JsonInclude, JsonPropertyName(FRIENDLY_KEY_VISIBLE_FROM)]
  public long? VisibleFrom { get; private set; }
  
  [BsonElement(DB_KEY_LANGUAGE), BsonIgnoreIfNull]
  [JsonInclude, JsonPropertyName(FRIENDLY_KEY_LANGUAGE)]
  public string Language { get; private set; }

  public StickyMessage(string text, long? durationInSeconds, long? expiration, long? visibleFrom, string language = "en-US")
  {
    Text = text;
    DurationInSeconds = durationInSeconds;
    Expiration = expiration;
    VisibleFrom = visibleFrom;
    Language = language;
  }
}