using System.Collections.Generic;
using MongoDB.Driver;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;
using TowerPortal.Models.Permissions;

namespace TowerPortal.Services;
public class AccountService : PlatformMongoService<Account>
{
    public AccountService() : base(collection: "accounts") { }

    public Account GetByEmail(string email) => _collection
        .Find(filter: account => account.Email == email)
        .FirstOrDefault();

    /// <summary>
    /// Validates the user's current permissions against what's in Mongo's database.  If the email is null or empty,
    /// an unauthorized permission set is returned.
    /// </summary>
    /// Note: We need to check to make sure the email isn't invalid because it could be possible that there's a problem,
    /// either in our code or in the auth provider's, such that we get a null response back.  Should that happen, any account
    /// that shares the same issue will then have shared permissions.  This is a pessimistic approach to prevent such
    /// users from gaining access to our system.
    public Passport CheckPermissions(Account account) => !string.IsNullOrWhiteSpace(account?.Email)
        ? GetByEmail(account.Email).Permissions
        : new Passport(Passport.PassportType.Unauthorized);

    public List<Account> GetAllAccounts() => _collection
        .Find(filter: account => true)
        .ToList();

    public long UpdatePassport(string accountId, Passport passport) => _collection.UpdateOne(
        filter: account => account.Id == accountId,
        update: Builders<Account>.Update.Set(field: account => account.Permissions, passport)
    ).ModifiedCount;
}