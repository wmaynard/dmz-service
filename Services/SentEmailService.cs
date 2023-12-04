using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Minq;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Data;

namespace Dmz.Services;

public class SentEmailService : MinqService<SentEmailData>
{
    public static SentEmailService Instance { get; private set; }

    public SentEmailService() : base("emailData") => Instance = this;

    public void Register(string email, string template) => mongo.Insert(new SentEmailData
    {
        Address = email,
        Template = template
    });

    public long CountSince(long timestamp) => mongo
        .Count(query => query
            .GreaterThan(email => email.SentOn, timestamp)
        );
}

public class SentEmailData : PlatformCollectionDocument
{
    [BsonElement("email")]
    public string Address { get; set; }
    
    [BsonElement("type")]
    public string Template { get; set; }
    
    [BsonElement("sent")]
    public long SentOn { get; private set; }

    public SentEmailData() => SentOn = Timestamp.Now;
}