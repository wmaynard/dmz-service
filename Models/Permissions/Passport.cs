using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dmz.Exceptions;
using Dmz.Models.Portal;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeAccessorOwnerBody

namespace Dmz.Models.Permissions;

/// <summary>
/// This class is representative of all the permissions a user can hold.  It contains helper properties to improve code readability.
/// Whenever a new PermissionGroup is created, the Passport needs to be given a new property to match it.  Accessing permissions is easy
/// via the Passport, e.g.: Passport.Mail.SendGlobalMessage.
/// </summary>
[BsonIgnoreExtraElements]
public class Passport : List<PermissionGroup>
{
    private static readonly string[] PROD_SUPERUSERS =
    {
        "william.maynard@rumbleentertainment.com"
    };
    private static readonly string[] DEV_SUPERUSERS =
    {
        "william.maynard@rumbleentertainment.com",
        "william.maynard@rumblegames.com"
    };

    private static readonly string[] READONLY_DOMAINS =
    {
        "rumbleentertainment.com",
        "rumblegames.com",
    };

    public CalendarPermissions     Calendar    => Fetch<CalendarPermissions>();
    public ChatPermissions         Chat        => Fetch<ChatPermissions>();
    public ConfigPermissions       Config      => Fetch<ConfigPermissions>();
    public LeaderboardsPermissions Leaderboard => Fetch<LeaderboardsPermissions>();
    public MailPermissions         Mail        => Fetch<MailPermissions>();
    public MatchmakingPermissions  Matchmaking => Fetch<MatchmakingPermissions>();
    public MultiplayerPermissions  Multiplayer => Fetch<MultiplayerPermissions>();
    public NftPermissions          Nft         => Fetch<NftPermissions>();
    public ReceiptPermissions      Receipt     => Fetch<ReceiptPermissions>();
    public PlayerPermissions       Player      => Fetch<PlayerPermissions>();
    public PortalPermissions       Portal      => Fetch<PortalPermissions>();
    public PvpPermissions          Pvp         => Fetch<PvpPermissions>();
    public TokenPermissions        Token       => Fetch<TokenPermissions>();

    private T Fetch<T>() where T : PermissionGroup
    {
        T output = this.OfType<T>().FirstOrDefault();
        if (output == null)
            base.Add(output = Activator.CreateInstance<T>());

        return output;
    }
    
    [BsonConstructor]
    public Passport(){}
    
    public Passport(PassportType userType = PassportType.Nonprivileged)
    {
        Type[] groups = Assembly
            .GetExecutingAssembly()
            .GetExportedTypes()
            .Where(type => type.IsAssignableTo(typeof(PermissionGroup)))
            .Where(type => !type.IsAbstract)
            .ToArray();
        foreach (Type type in groups)
        {
            PermissionGroup group = (PermissionGroup)Activator.CreateInstance(type);
            switch (userType)
            {
                case PassportType.Superuser:
                    group?.UpdatePermissions(true);
                    break;
                case PassportType.Readonly:
                    group?.UpdatePermissions(true, filter: "View_");
                    break;
                case PassportType.Nonprivileged:
                    group?.UpdatePermissions(false);
                    break;
                case PassportType.Unauthorized:
                default:
                    throw new PermissionInvalidException();
            }
            base.Add(item: group);
        }
    }
    
    public new void Add(PermissionGroup toAdd)
    {
        PermissionGroup current = this.FirstOrDefault(group => group.GetType() == toAdd.GetType());
        if (current == null)
            base.Add(toAdd);
        else
            current.Merge(toAdd);
    }
    public enum PassportType { Superuser, Readonly, Nonprivileged, Unauthorized }

    private static Passport GetDefaultPermissions(SsoData data, TokenInfo token)
    {
        if (data == null && token == null)
            throw new PlatformException("Unauthorized.");

        bool isSuperUser = PROD_SUPERUSERS.Contains(data?.Email ?? token.Email)
            || (!PlatformEnvironment.IsProd && DEV_SUPERUSERS.Contains(data?.Email ?? token.Email));
        
        if (isSuperUser)
            return new Passport(PassportType.Superuser);
        
        string tokenDomain = token?.Email[(token.Email.IndexOf('@') + 1)..] ?? data?.Domain;
        if (string.IsNullOrWhiteSpace(tokenDomain))
            throw new PlatformException("Unauthorized.");
        
        if (READONLY_DOMAINS.Contains(tokenDomain))
            return new Passport(PassportType.Readonly);
        
        string[] dcDomains = DynamicConfig.Instance
            ?.GetValuesFor(Audience.PlayerService)
            .Optional<string>("allowedSignupDomains")
            ?.Split(',')
            ?? Array.Empty<string>();

        
        if (!string.IsNullOrWhiteSpace(tokenDomain) && dcDomains.Any(domain => tokenDomain.Contains(domain)))
            return new Passport(PassportType.Readonly);

        throw new PlatformException("Unauthorized.");
    }

    public static Passport GetDefaultPermissions(SsoData data) => GetDefaultPermissions(data, null);
    public static Passport GetDefaultPermissions(TokenInfo token) => GetDefaultPermissions(null, token);

    public void Merge(Passport other)
    {
        foreach (PermissionGroup group in this)
            group.Merge(other.FirstOrDefault(pg => pg.Name == group.Name));
    }

    public static Passport FromPermissionSet(RumbleJson permissions)
    {
        if (permissions == null)
            return null;
        Passport output = new(PassportType.Readonly);

        foreach (PermissionGroup group in output)
            group.UpdateFromValues(permissions);

        return output;
    }
}