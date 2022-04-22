using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models;

[BsonIgnoreExtraElements]
public class GlobalMessage : Message
{
    internal const string DB_KEY_FOR_ACCOUNTS_BEFORE = "acctsbefore";

    public const string FRIENDLY_KEY_FOR_ACCOUNTS_BEFORE = "forAccountsBefore";
    
    [BsonElement(DB_KEY_FOR_ACCOUNTS_BEFORE)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_FOR_ACCOUNTS_BEFORE)]
    public long? ForAccountsBefore { get; private set; }

    public GlobalMessage(string subject, string body, List<Attachment> attachments, long expiration,
        long visibleFrom, string icon, string banner, StatusType status, string internalNote, long? forAccountsBefore = null)
        : base(subject: subject, body: body, attachments: attachments, expiration: expiration,
            visibleFrom: visibleFrom, icon: icon, banner: banner, status: status, internalNote: internalNote)
    {
        ForAccountsBefore = forAccountsBefore;
    }
}