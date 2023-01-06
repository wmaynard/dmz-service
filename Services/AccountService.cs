using System;
using System.Collections.Generic;
using System.Linq;
using Dmz.Exceptions;
using Dmz.Models.Permissions;
using Dmz.Models.Portal;
using MongoDB.Driver;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
// ReSharper disable ArrangeMethodOrOperatorBody

namespace Dmz.Services;
public class AccountService : PlatformMongoService<Account>
{
    public AccountService() : base(collection: "accounts") { }

    public Account FindById(string id) => FindOne(filter: account => account.Id == id);

    public List<Account> FindByRole(string name)
    {
        return _collection.Find(Builders<Account>.Filter.ElemMatch(account => account.Roles, role => role.Name == name)).ToList();
    }

    public List<Account> FindByPermission(string perm)
    {
        List<Account> accounts = _collection.Find(acc => true).ToList();
        List<Account> matchingAccounts = new List<Account>();

        foreach (Account account in accounts)
        {
            bool found = false;
            foreach (PermissionGroup permissionGroup in account.Permissions)
            {
                bool inPermissions;
                permissionGroup.Values.TryGetValue(perm, out inPermissions);
                if (inPermissions)
                {
                    found = true;
                }
            }

            foreach (Role role in account.Roles)
            {
                foreach (PermissionGroup permissionGroup in role.Permissions)
                {
                    bool inRoles;
                    permissionGroup.Values.TryGetValue(perm, out inRoles);
                    if (inRoles)
                    {
                        found = true;
                    }
                }
            }

            if (found)
            {
                matchingAccounts.Add(account);
            }
        }
        
        return matchingAccounts;
    }

    public Account GetByEmail(string email) => _collection
        .Find(filter: account => account.Email == email)
        .FirstOrDefault()
        ?? throw new PlatformException("No account found for specified email address.");

    public long UpdatePassport(string accountId, Passport passport) => _collection.UpdateOne(
        filter: account => account.Id == accountId,
        update: Builders<Account>.Update.Set(field: account => account.Permissions, passport)
    ).ModifiedCount;

    public Account GoogleLogin(SsoData data) => FindOne(filter: account => account.Email == data.Email) 
        ?? Create(Account.FromSsoData(data));
    
    public Account FindByToken(TokenInfo token) => token?.Email != null
        ? _collection
            .Find(account => account.Email == token.Email)
            .FirstOrDefault()
            ?? throw new AccountNotFoundException(token)
        : throw new InvalidTokenException(token?.Authorization, HttpContext.Request.Path.ToString());
    
    public void UpdateEditedRole(Role role)
    {
        List<WriteModel<Account>> listWrites = new List<WriteModel<Account>>();

        FilterDefinition<Account> filter = Builders<Account>.Filter.ElemMatch(account => account.Roles,oldRole => oldRole.Name == role.Name);
        UpdateDefinition<Account> update = Builders<Account>.Update.Set(account => account.Roles[-1], role);
		
        listWrites.Add(new UpdateManyModel<Account>(filter, update));
        _collection.BulkWrite(listWrites);
    }

    public void RemoveDeletedRole(string roleName)
    {
        List<WriteModel<Account>> listWrites = new List<WriteModel<Account>>();

        FilterDefinition<Account> filter = Builders<Account>.Filter.ElemMatch(account => account.Roles, role => role.Name == roleName);
        UpdateDefinition<Account> update = Builders<Account>.Update.PullFilter(account => account.Roles, role => role.Name == roleName);
		
        listWrites.Add(new UpdateManyModel<Account>(filter, update));
        _collection.BulkWrite(listWrites);
    }
}