using System.Text.Json.Serialization;
using Rumble.Platform.Data;

namespace Dmz.Models.Bounces;
public class BounceRecipient : PlatformDataModel
{
    [JsonPropertyName("emailAddress")]
    public string Address { get; set; }
    
    [JsonPropertyName("action")]
    public string Action { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("code")]
    public int Code => int.TryParse(Status.Replace(".", ""), out int output)
        ? output
        : -1;
    
    [JsonPropertyName("diagnosticCode")]
    public string Detail { get; set; }
}