using System.Text.Json.Serialization;
using Rumble.Platform.Common.Utilities.JsonTools;

namespace Dmz.Interop;

public class MongoWhitelistLink : PlatformDataModel
{
    [JsonPropertyName("href")]
    public string Url { get; set; }
    
    [JsonPropertyName("rel")]
    public string Relationship { get; set; }
}