using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities.JsonTools;

namespace Dmz.Services;

// TD-17262: Support unsubscribe flow
public class SubscriptionService : PlatformMongoService<SubscriptionService.SubscriptionStatus>
{
    public static SubscriptionService Instance { get; private set; }

    public SubscriptionService() : base("unsubscriptions") => Instance = this;
    
    public bool IsUnsubscribed(string email) => _collection
        .FindOneAndUpdate(
            filter: Builders<SubscriptionStatus>.Filter.Eq(status => status.Email, email),
            update: Builders<SubscriptionStatus>.Update.Set(status => status.Email, email),
            options: new FindOneAndUpdateOptions<SubscriptionStatus>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            }
        ).IsUnsubscribed;
    
    public SubscriptionStatus Unsubscribe(string email) => _collection
        .FindOneAndUpdate(
            filter: Builders<SubscriptionStatus>.Filter.Eq(status => status.Email, email),
            update: Builders<SubscriptionStatus>.Update
                .Set(status => status.Email, email)
                .Set(status => status.IsUnsubscribed, true),
            options: new FindOneAndUpdateOptions<SubscriptionStatus>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            }
        );
    
    public SubscriptionStatus Resubscribe(string email) => _collection
        .FindOneAndUpdate(
            filter: Builders<SubscriptionStatus>.Filter.Eq(status => status.Email, email),
            update: Builders<SubscriptionStatus>.Update
                .Set(status => status.Email, email)
                .Set(status => status.IsUnsubscribed, false),
            options: new FindOneAndUpdateOptions<SubscriptionStatus>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            }
        );
    
    public class SubscriptionStatus : PlatformCollectionDocument
    {
        [BsonElement("addr")]
        [SimpleIndex(unique: true)]
        public string Email { get; set; }
        
        [BsonElement("unsub")]
        public bool IsUnsubscribed { get; set; }
    }
}

