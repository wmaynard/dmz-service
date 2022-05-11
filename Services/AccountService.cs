using System.Collections.Generic;
using MongoDB.Driver;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;

namespace TowerPortal.Services;
public class AccountService : PlatformMongoService<Account>
{
    public AccountService() : base(collection: "accounts")
    { }

    public Account GetByEmail(string email)
    {
        return _collection.Find(filter: account => account.Email == email).FirstOrDefault();
    }
    
    // TODO Refactor to return all permissions for the user at once
    // Currently this is called for every type of permission
    // Fetch list of roles from db, return list and have controller check list for role
    public string CheckPermissions(Account account, string role)
    {
        //string env = PlatformEnvironment.Optional<string>("env"); // get env from dynamic config?
        string env = PlatformEnvironment.Optional<string>(key: "RUMBLE_DEPLOYMENT");
        
        Account acc = GetByEmail(account.Email);

        if (acc.Roles.Contains(role + env))
        {
            return "true"; // ViewData is weird with booleans, so using string
        }
        return null;
    }

    public List<Account> GetAllAccounts()
    {
        return _collection.Find(account => true).ToList();
    }

    public void UpdateRoles(Account acc, List<string> roles)
    {
        List<WriteModel<Account>> listWrites = new List<WriteModel<Account>>();
        
        FilterDefinition<Account> filter =
            Builders<Account>.Filter.Where(account => account.Email == acc.Email);
        UpdateDefinition<Account> update = Builders<Account>.Update.Set(account => account.Roles, roles);
        
        listWrites.Add(new UpdateOneModel<Account>(filter, update));
        _collection.BulkWrite(listWrites);
    }
}