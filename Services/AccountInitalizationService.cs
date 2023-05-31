using Dmz.Models.Player;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Data;

namespace Dmz.Services;

public class AccountInitializationService : PlatformMongoService<InitializationRecord>
{
    public AccountInitializationService() : base("initializations") { }

    public void MarkAsInitialized(string accountId) => _collection
        .FindOneAndUpdate(
            filter: Builders<InitializationRecord>.Filter.Eq(record => record.AccountId, accountId),
            update: Builders<InitializationRecord>.Update
                .Set(record => record.AccountId, accountId)
                .Set(record => record.Initialized, true),
            options: new FindOneAndUpdateOptions<InitializationRecord>
            {
                IsUpsert = true
            }
        );
    
    public bool IsInitialized(string accountId) => _collection
        .Find(Builders<InitializationRecord>.Filter.Eq(record => record.AccountId, accountId))
        .FirstOrDefault()
        ?.Initialized
        ?? false;
}