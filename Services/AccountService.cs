using System.Collections.Generic;
using MongoDB.Driver;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using TowerPortal.Exceptions;
using TowerPortal.Models;
using TowerPortal.Models.Permissions;
using TowerPortal.Models.Portal;

namespace TowerPortal.Services;
public class AccountService : PlatformMongoService<Account>
{
    public AccountService() : base(collection: "accounts") { }

    public Account FindById(string id) => FindOne(filter: account => account.Id == id);

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
}