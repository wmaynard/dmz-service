using Dmz.Models.Player;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Services;

public class AccountInitializationService : PlatformMongoService<InitializationRecord>
{
    private readonly ApiService _apiService;

    public AccountInitializationService(ApiService apiService) : base("initializations")
        => _apiService = apiService;

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

    public void InitializeIfNeeded(RumbleJson player, out string error)
    {
        string accountId = player.Require<string>("id");
        string token = player.Require<string>("token");
        string outputError = null;
        
        if (!IsInitialized(accountId))
            _apiService
                .Request("/game/initAccount")
                .AddHeader("Authorization", token) // TODO: Switch to .AddAuthorization after game server doesn't break on "Bearer " headers
                .OnSuccess(response =>
                {
                    if (response.Require<bool>("success"))
                    {
                        Log.Info(Owner.Will, "Account initialized through DMZ successfully.", data: new
                        {
                            AccountId = accountId
                        });
                        MarkAsInitialized(accountId);
                    }
                    else
                    {
                        outputError = response.Optional<string>("error") ?? "There was an unknown problem initializing the account.";
                        Log.Error(Owner.Will, "Unable to initialize account.", data: new
                        {
                            Player = player,
                            Response = response,
                            Error = outputError
                        });
                    }
                })
                .OnFailure(response =>
                {
                    outputError = "Unable to initialize account.";
                    Log.Error(Owner.Will, "Unable to initialize account.", data: new
                    {
                        Player = player,
                        Response = response
                    });
                })
                .Get();
        error = outputError;
    }
}