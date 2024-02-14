using System;
using System.Text.Json.Serialization;
using Dmz.Models;
using Dmz.Utilities;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RCL.Logging;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Services;

public class OtpService : PlatformMongoService<StoredValue>
{
    private readonly ApiService _apiService;
    private readonly DynamicConfig _dynamicConfig;

    public OtpService(ApiService apiService, DynamicConfig dynamicConfig) : base("otpStore")
    {
        _apiService = apiService;
        _dynamicConfig = dynamicConfig;
    }

    /// <summary>
    /// Stores a value and returns its Mongo ID, which functions as a one-time password.
    /// </summary>
    /// <param name="value">The value to store.</param>
    /// <returns></returns>
    /// <exception cref="PlatformException"></exception>
    public string Store(StoredValue toStore)
    {
        if (string.IsNullOrWhiteSpace(toStore.Value))
            throw new PlatformException("Value cannot be blank.");

        _collection.InsertOne(toStore);
        return toStore.Id;
    }

    public StoredValue Retrieve(string id)
    {
        try
        {
            StoredValue output = _collection.FindOneAndUpdate(
                filter: stored => stored.Id == id && !stored.Claimed,
                update: Builders<StoredValue>.Update.Combine(
                    Builders<StoredValue>.Update.Set(stored => stored.Claimed, true),
                    Builders<StoredValue>.Update.Set(stored => stored.Value, null)
                ));

            if (output != null)
                return output;

            StoredValue used = _collection
                .Find(stored => stored.Id == id)
                .FirstOrDefault()
                ?? throw new PlatformException("OTP does not exist", code: ErrorCode.MongoRecordNotFound);
            
            // Log.Error(Owner.Will, "OTP was used more than once.  Ensure frontend is not requesting it multiple times.", data: new
            // {
            //     otp = used.Id
            // });

            // InvalidateTokens(used.AccountId);
            throw new PlatformException("OTP was already consumed.", code: ErrorCode.Unauthorized);
        }
        catch (FormatException)
        {
            throw new PlatformException("OTP not found.", code: ErrorCode.MongoRecordNotFound);
        }
    }
    
    /// <summary>
    /// Extra security measure for ultimate hardening.  If an OTP is used multiple times, this can be used
    /// to guarantee the user's account remains safe, though it would interrupt their existing gameplay / web
    /// interactions by requiring a logout.
    /// </summary>
    /// <param name="accountId">The account to force a relog for.</param>
    private void InvalidateTokens(string accountId) => _apiService
        .Request("/token/invalidate")
        .AddAuthorization(_dynamicConfig.AdminToken)
        .SetPayload(new RumbleJson
        {
            { "aid", accountId }
        })
        .OnFailure(response =>
        {
            Log.Error(Owner.Will, "Could not invalidate token after OTP was already used.", data: new
            {
                Response = response
            });
        })
        .OnSuccess(response =>
        {
            Log.Warn(Owner.Will, "User tokens invalidated because OTP was already used.");
            _collection.DeleteOne(filter: stored => stored.Id == accountId);
        })
        .Patch();

    private const int FOUR_DAYS = 4 * 24 * 60 * 60;

    public long Cleanup() => _collection.DeleteMany(
        filter: Builders<StoredValue>.Filter.Lte(stored => stored.CreatedOn, Timestamp.Now - FOUR_DAYS)
    ).DeletedCount;
}