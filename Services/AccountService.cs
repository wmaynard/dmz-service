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
    
    public Passport CheckPermissions(Account account)
    {
        Account acc = GetByEmail(account.Email);

        return acc.Permissions;
    }

    public List<Account> GetAllAccounts() => _collection
        .Find(filter: account => true)
        .ToList();

    // The following is removed in favor of permissions
    // public void UpdateRoles(Account acc, List<string> roles)
    // {
    //     List<WriteModel<Account>> listWrites = new List<WriteModel<Account>>();
    //     
    //     FilterDefinition<Account> filter =
    //         Builders<Account>.Filter.Where(account => account.Email == acc.Email);
    //     UpdateDefinition<Account> update = Builders<Account>.Update.Set(account => account.Roles, roles);
    //     
    //     listWrites.Add(new UpdateOneModel<Account>(filter, update));
    //     _collection.BulkWrite(listWrites);
    // }
}