using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Data;

namespace Dmz.Models.Player;

public class InitializationRecord : PlatformCollectionDocument
{
    [BsonElement("aid")]
    [SimpleIndex(unique: true)]
    public string AccountId { get; set; }
    
    [BsonElement("init")]
    public bool Initialized { get; set; }
}