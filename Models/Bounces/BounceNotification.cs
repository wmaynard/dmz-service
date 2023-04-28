using System;
using System.Linq;
using System.Text.Json.Serialization;
using Amazon.SQS.Model;
using Dmz.Services;
using Rumble.Platform.Data;

namespace Dmz.Models.Bounces;

public class BounceNotification : PlatformDataModel
{
    [JsonPropertyName("Type")]
    public string Type { get; set; }
    
    [JsonPropertyName("MessageId")]
    public string MessageId { get; set; }
    
    [JsonPropertyName("TopicArn")]
    public string Topic { get; set; }
    
    [JsonPropertyName("Subject")]
    public string Subject { get; set; }
    
    [JsonPropertyName("Timestamp")]
    public DateTime Date { get; set; }
    
    [JsonPropertyName("ts"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Timestamp => Date != default
        ? (long)(Date - DateTime.UnixEpoch).TotalSeconds
        : default;
    
    [JsonInclude, JsonPropertyName("Message")] // TODO: Does this look different for complaints?
    public RumbleJson Message { private get; set; }
    
    [JsonIgnore]
    public BounceMessage BounceMessage => Message.ToModel<BounceMessage>();
    
    [JsonIgnore]
    public ComplaintMessage ComplaintMessage => throw new NotImplementedException();
    
    [JsonPropertyName("SignatureVersion")]
    public string SignatureVersion { get; set; }
    
    [JsonPropertyName("Signature")]
    public string Signature { get; set; }
    
    [JsonPropertyName("SigningCertURL")]
    public string CertificateUrl { get; set; }
    
    [JsonPropertyName("UnsubscribeURL")]
    public string UnsubscribeUrl { get; set; }

    [JsonIgnore]
    public string ReceiptHandle { get; private set; }
    
    private BounceNotification SetReceiptHandle(string receipt)
    {
        ReceiptHandle = receipt;
        return this;
    }

    public static BounceNotification[] FromSqsResponse(ReceiveMessageResponse response) => response
        .Messages
        .Select(message => ((RumbleJson)message.Body).ToModel<BounceNotification>()?.SetReceiptHandle(message.ReceiptHandle))
        .ToArray();
}