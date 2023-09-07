using System.Linq;
using System.Text.Json.Serialization;
using Rumble.Platform.Data;

namespace Dmz.Interop;

public class MongoWhitelistEntry : PlatformDataModel
{
    [JsonPropertyName("cidrBlock")]
    public string CidrBlock { get; set; }
    
    [JsonPropertyName("comment")]
    public string Comment { get; set; }
    
    [JsonPropertyName("groupId")]
    public string GroupId { get; set; }
    
    [JsonPropertyName("ipAddress")]
    public string IpAddress { get; set; }
    
    [JsonPropertyName("links")]
    public MongoWhitelistLink[] Links { get; set; }

    [JsonIgnore]
    public string SelfReferenceUrl => Links?.FirstOrDefault(link => link.Relationship == "self")?.Url;
}