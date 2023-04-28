using System.Text.Json.Serialization;
using Rumble.Platform.Data;

namespace Dmz.Models.Bounces;

public class BounceMessage : PlatformDataModel
{
    public const string JSON_KEY_IN_PARENT = "Message";
    
    [JsonPropertyName("eventType")]
    public string EventType { get; set; }
    
    [JsonPropertyName("bounce")]
    public Bounce Bounce { private get; set; }

    [JsonPropertyName("mail")]
    public RumbleJson MailDetails { get; set; }
    
    [JsonPropertyName("feedbackId")]
    public string FeedbackId { get; set; }
    
    #region ChildAccessors
    [JsonPropertyName("bounceType")]
    public string Type => Bounce?.Type;
        
    [JsonPropertyName("bounceSubType")]
    public string SubType => Bounce?.SubType;

    [JsonPropertyName("bouncedRecipients")]
    public BounceRecipient[] Recipients => Bounce?.Recipients;
    
    [JsonPropertyName("failedAddresses")]
    public string[] Addresses => Bounce?.Addresses;

    [JsonPropertyName("ts"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Timestamp => Bounce?.Timestamp ?? default;
    
    [JsonPropertyName("reportingMTA")]
    public string Reporter => Bounce?.Reporter;
    #endregion ChildAccessors
}