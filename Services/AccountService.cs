using System.Collections.Generic;
using MongoDB.Driver;
using Rumble.Platform.Common.Services;
using TowerPortal.Models.Permissions;
using TowerPortal.Models.Portal;

namespace TowerPortal.Services;
public class AccountService : PlatformMongoService<Account>
{
    public AccountService() : base(collection: "accounts") { }

    public Account FindById(string id) => FindOne(filter: account => account.Id == id);

    public Account GetByEmail(string email) => _collection
        .Find(filter: account => account.Email == email)
        .FirstOrDefault();

    public List<Account> GetAllAccounts() => _collection
        .Find(filter: account => true)
        .ToList();

    public long UpdatePassport(string accountId, Passport passport) => _collection.UpdateOne(
        filter: account => account.Id == accountId,
        update: Builders<Account>.Update.Set(field: account => account.Permissions, passport)
    ).ModifiedCount;
}