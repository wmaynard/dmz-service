using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities.JsonTools;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeAttributes
// ReSharper disable ArrangeAccessorOwnerBody

namespace Dmz.Models.Mailbox;

[BsonIgnoreExtraElements]
public class Message : PlatformCollectionDocument
{
    internal const string DB_KEY_SUBJECT = "sbjct";
    internal const string DB_KEY_BODY = "body";
    internal const string DB_KEY_ATTACHMENTS = "attchmnts";
    internal const string DB_KEY_TIMESTAMP = "tmestmp";
    internal const string DB_KEY_EXPIRATION = "expire";
    internal const string DB_KEY_VISIBLE_FROM = "visible";
    internal const string DB_KEY_ICON = "icon";
    internal const string DB_KEY_BANNER = "banner";
    internal const string DB_KEY_STATUS = "status";
    internal const string DB_KEY_INTERNAL_NOTE = "note";
    internal const string DB_KEY_PREVIOUS_VERSIONS = "prev";

    public const string FRIENDLY_KEY_SUBJECT = "subject";
    public const string FRIENDLY_KEY_BODY = "body";
    public const string FRIENDLY_KEY_ATTACHMENTS = "attachments";
    public const string FRIENDLY_KEY_TIMESTAMP = "timestamp";
    public const string FRIENDLY_KEY_EXPIRATION = "expiration";
    public const string FRIENDLY_KEY_VISIBLE_FROM = "visibleFrom";
    public const string FRIENDLY_KEY_ICON = "icon";
    public const string FRIENDLY_KEY_BANNER = "banner";
    public const string FRIENDLY_KEY_STATUS = "status";
    public const string FRIENDLY_KEY_INTERNAL_NOTE = "internalNote";
    public const string FRIENDLY_KEY_PREVIOUS_VERSIONS = "previousVersions";

    [BsonElement(DB_KEY_SUBJECT)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_SUBJECT)]
    public string Subject { get; private set; }
    
    [BsonElement(DB_KEY_BODY)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_BODY)]
    public string Body { get; private set; }
    
    [BsonElement(DB_KEY_ATTACHMENTS)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ATTACHMENTS)]
    public List<Attachment> Attachments { get; private set;}
    
    [BsonElement(DB_KEY_TIMESTAMP)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_TIMESTAMP)]
    public long Timestamp { get; private set; }
    
    [BsonElement(DB_KEY_EXPIRATION)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_EXPIRATION)]
    public long Expiration { get; private set; }
    
    [BsonElement(DB_KEY_VISIBLE_FROM)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_VISIBLE_FROM)]
    public long VisibleFrom { get; private set; }
    
    [BsonElement(DB_KEY_ICON), BsonDefaultValue(null)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ICON)]
    public string Icon { get; private set; }
    
    [BsonElement(DB_KEY_BANNER), BsonDefaultValue(null)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_BANNER)]
    public string Banner { get; private set; }
    
    public enum StatusType { UNCLAIMED, CLAIMED }
    [BsonElement(DB_KEY_STATUS)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_STATUS)]
    public StatusType Status { get; private set; }
    
    [BsonElement(DB_KEY_INTERNAL_NOTE), BsonDefaultValue(null)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_INTERNAL_NOTE)]
    public string InternalNote { get; private set; }
    
    [BsonElement(DB_KEY_PREVIOUS_VERSIONS), BsonIgnoreIfNull]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_PREVIOUS_VERSIONS)]
    public List<Message> PreviousVersions { get; private set; }

    [BsonIgnore]
    [JsonIgnore]
    public bool IsExpired => Expiration <= Rumble.Platform.Common.Utilities.Timestamp.Now; // no setter, change expiration to UnixTime instead

    public Message(string subject, string body, List<Attachment> attachments, long expiration, long visibleFrom, string icon, string banner, StatusType status, string internalNote)
    {
        Subject = subject;
        Body = body;
        Attachments = attachments;
        Timestamp = Rumble.Platform.Common.Utilities.Timestamp.Now;
        Expiration = expiration;
        VisibleFrom = visibleFrom;
        Icon = icon;
        if (icon == null)
        {
            Icon = "";
        }
        Banner = banner;
        if (banner == null)
        {
            Banner = "";
        }
        Status = status;
        InternalNote = internalNote;
        PreviousVersions = new List<Message>();
        Id = ObjectId.GenerateNewId().ToString(); // potential overlap with GlobalMessage?
    }
}