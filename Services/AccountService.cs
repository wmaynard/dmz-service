using System;
using System.Collections.Generic;
using System.Linq;
using Dmz.Exceptions;
using Dmz.Models.Permissions;
using Dmz.Models.Portal;
using RCL.Logging;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Minq;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities;

namespace Dmz.Services;

public class AccountService : MinqService<Account>
{
    private readonly RoleService _roles;
    public static AccountService Instance { get; private set; }

    public AccountService(RoleService roles) : base("accounts")
    {
        Instance = this;
        _roles = roles;
    }

    public Account[] All()
    {
        Account[] output = mongo
            .All()
            .ToArray();

        string[] roleIds = output
            .Where(account => account.RoleIds != null && account.RoleIds.Length > 0)
            .SelectMany(account => account.RoleIds)
            .Where(id => !string.IsNullOrWhiteSpace(id) && id.CanBeMongoId())
            .ToArray();

        Role[] roles = _roles.FromIds(roleIds).ToArray();

        foreach (Account account in output)
        {
            account.RoleIds ??= Array.Empty<string>();
            account.Roles = roles
                .Where(role => account.RoleIds.Contains(role.Id))
                .ToArray();
        }

        return output;
    }

    public override Account FromId(string id)
    {
        Account output = base.FromId(id);
        
        output.Roles = _roles.FromIds(output.RoleIds).ToArray();

        return output;
    }

    public bool UpdatePassport(string id, string[] roleIds, Passport passport)
    {
        if (roleIds == null && passport == null)
            throw new PlatformException($"You must provide either '{Account.FRIENDLY_KEY_ROLES}', '{Account.FRIENDLY_KEY_PERMISSIONS}', or both.");

        return mongo
            .ExactId(id)
            .Update(update =>
            {
                if (roleIds != null)
                    update.Set(account => account.RoleIds, roleIds);
                if (passport != null)
                    update.Set(account => account.Permissions, passport);
            }) == 1;
    }

    public Account GoogleLogin(SsoData data) => mongo
        .Where(query => query.EqualTo(account => account.Email, data.Email))
        .Upsert(update => update
            .SetOnInsert(account => account.Name, data.Name)
            .SetOnInsert(account => account.Email, data.Email)
            .SetOnInsert(account => account.Permissions, Passport.GetDefaultPermissions(data))
            .SetOnInsert(account => account.RoleIds, Array.Empty<string>())
        );
    
    public Account FindByToken(TokenInfo token) => string.IsNullOrWhiteSpace(token?.Email)
        ? throw new InvalidTokenException(token?.Authorization, "deprecated")
        : mongo
            .Where(query => query.EqualTo(account => account.Email, token.Email))
            .FirstOrDefault()
            ?? throw new AccountNotFoundException(token);

    public Account FromToken(TokenInfo token, bool loadRoles = false)
    {
        Account output = mongo
            .Where(query => query.EqualTo(account => account.Email, token.Email))
            .Upsert(update => update
                .SetOnInsert(account => account.Permissions, Passport.GetDefaultPermissions(token))
                .SetOnInsert(account => account.RoleIds, Array.Empty<string>())
            );

        if (output != null && loadRoles)
            output.LoadPermissionsFrom(_roles.FromIds(output.RoleIds));
        
        return output;
    }

    public long RemoveRole(string roleId) => mongo
        .All()
        .Update(query => query.RemoveItems(account => account.RoleIds, roleId));

    public AuditLog[] GetActivityLogs() => mongo
        .All()
        .Project(account => account.Activity)
        .SelectMany(_ => _)
        .OrderByDescending(log => log.Time)
        .ToArray();

    public bool AddLog(AuditLog log) => mongo
        .ExactId(log.PortalAccountId)
        .Update(update => update.AddItems(account => account.Activity, limitToKeep: 1_000, log)) == 1;

}