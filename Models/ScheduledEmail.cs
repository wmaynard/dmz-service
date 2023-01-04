using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Data;

namespace Dmz.Models;

public class ScheduledEmail : PlatformCollectionDocument
{
    private const string INDEX_UNSENT = "find_unsent";
    private const string INDEX_SENT = "delete_sent";

    public const string DB_KEY_SENT = "sent";
    public const string DB_KEY_TIMESTAMP = "ts";
    public const string DB_KEY_ATTEMPTS = "attempts";
    public const string DB_KEY_ADDRESS = "email";
    public const string DB_KEY_SUBJECT = "sbj";
    public const string DB_KEY_HTML = "html";
    public const string DB_KEY_TEXT = "txt";
    public const string DB_KEY_DELAY = "wait";
    
    public const string FRIENDLY_KEY_SENT = "sent";
    public const string FRIENDLY_KEY_TIMESTAMP = "timestamp";
    public const string FRIENDLY_KEY_ATTEMPTS = "attempts";
    public const string FRIENDLY_KEY_ADDRESS = "email";
    public const string FRIENDLY_KEY_SUBJECT = "subject";
    public const string FRIENDLY_KEY_HTML = "html";
    public const string FRIENDLY_KEY_TEXT = "text";
    public const string FRIENDLY_KEY_DELAY = "sendAfter";
    
    [BsonElement(DB_KEY_SENT)]
    [JsonPropertyName(FRIENDLY_KEY_SENT), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [CompoundIndex(group: INDEX_UNSENT, priority: 1)]
    [CompoundIndex(group: INDEX_SENT, priority: 1)]
    public bool Sent { get; set; }
    
    [BsonElement(DB_KEY_TIMESTAMP)]
    [JsonPropertyName(DB_KEY_TIMESTAMP), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [CompoundIndex(group: INDEX_SENT, priority: 2)]
    public long SentOn { get; set; }
    
    [BsonElement(DB_KEY_ATTEMPTS)]
    [JsonPropertyName(FRIENDLY_KEY_ATTEMPTS), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [CompoundIndex(group: INDEX_UNSENT, priority: 2)]
    public int Attempts { get; set; }
    
    [BsonElement(DB_KEY_ADDRESS), BsonRequired]
    [JsonPropertyName(FRIENDLY_KEY_ADDRESS)]
    public string Address { get; set; }
    
    [BsonElement(DB_KEY_SUBJECT), BsonRequired]
    [JsonPropertyName(FRIENDLY_KEY_SUBJECT)]
    public string Subject { get; set; }
    
    [BsonElement(DB_KEY_HTML), BsonRequired]
    [JsonPropertyName(FRIENDLY_KEY_SUBJECT)]
    public string Html { get; set; }
    
    [BsonElement(DB_KEY_TEXT), BsonRequired]
    [JsonPropertyName(FRIENDLY_KEY_TEXT)]
    public string Text { get; set; }
    
    [BsonElement(DB_KEY_DELAY)]
    [JsonPropertyName(FRIENDLY_KEY_DELAY)]
    [CompoundIndex(group: INDEX_UNSENT, priority: 3)]
    public long SendAfter { get; set; }
}