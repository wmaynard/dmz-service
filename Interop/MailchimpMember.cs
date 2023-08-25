using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Data;

namespace Dmz.Interop;

public class MailchimpMember : PlatformCollectionDocument
{
    public const string KEY_PARENT_OBJECT = "members";
    public const string KEY_EMAIL_ADDRESS = "email_address";
    public const string KEY_SUBSCRIPTION_STATUS = "status";
    public const string KEY_MEMBER_ID = "id";
    public const string KEY_ADDITIONAL_DATA = "merge_fields";
    
    [BsonElement("email")]
    [JsonPropertyName(KEY_EMAIL_ADDRESS)]
    public string Email { get; set; }
    
    [BsonElement("status")]
    [JsonPropertyName(KEY_SUBSCRIPTION_STATUS)]
    public string Status { get; set; }
    
    [BsonElement("created")]
    public long CreatedOn { get; set; }
    
    [BsonElement(TokenInfo.DB_KEY_ACCOUNT_ID)]
    [JsonPropertyName(TokenInfo.FRIENDLY_KEY_ACCOUNT_ID)]
    public string AccountId { get; set; }
    
    [BsonElement("sn")]
    public string Screenname { get; set; }
    
    [BsonIgnore]
    [JsonPropertyName(KEY_ADDITIONAL_DATA)]
    public MergeFields AdditionalData { get; set; }
    
    [BsonElement("claimed")]
    public List<string> ClaimedEmails { get; set; }

    public MailchimpSubscriptionStatus SubscriptionStatus => Status switch
    {
        "unsubscribed" => MailchimpSubscriptionStatus.Unsubscribed,
        "subscribed" => MailchimpSubscriptionStatus.Subscribed,
        _ => MailchimpSubscriptionStatus.Unknown
    };

    public static void AssignDownstreamValues(ref MailchimpMember[] array)
    {
        foreach (MailchimpMember member in array)
        {
            member.AccountId ??= member.AdditionalData?.AccountId;
            member.Screenname ??= member.AdditionalData?.Screenname;
        }
    }

    public class MergeFields : PlatformDataModel
    {
        [JsonPropertyName("FNAME")]
        public string Screenname { get; set; }
    
        [JsonPropertyName("PHONE")]
        public string AccountId { get; set; }
    }
}

