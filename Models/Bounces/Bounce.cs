using System;
using System.Linq;
using System.Text.Json.Serialization;
using Dmz.Services;
using Rumble.Platform.Common.Utilities.JsonTools;

namespace Dmz.Models.Bounces;

public class Bounce : PlatformDataModel
{
    [JsonPropertyName("feedbackId")]
    public string FeedbackId { get; set; }
        
    [JsonPropertyName("bounceType")]
    public string Type { get; set; }
        
    [JsonPropertyName("bounceSubType")]
    public string SubType { get; set; }
        
    [JsonPropertyName("bouncedRecipients")]
    public BounceRecipient[] Recipients { get; set; }
    
    [JsonPropertyName("failedAddresses")]
    public string[] Addresses => Recipients.Select(recipient => recipient.Address).ToArray();
    
    [JsonPropertyName("timestamp")]
    public DateTime Date { get; set; }

    [JsonPropertyName("ts"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Timestamp => Date != default
        ? (long)(Date - DateTime.UnixEpoch).TotalSeconds
        : default;
    
    [JsonPropertyName("reportingMTA")]
    public string Reporter { get; set; }
}