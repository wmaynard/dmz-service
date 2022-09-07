using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dmz.Exceptions;
using Dmz.Models.Portal;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Utilities;

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
        "nathan.mac@rumbleentertainment.com",
        "william.maynard@rumbleentertainment.com",
        "william.maynard@rumblegames.com",
        "mark.spenner@rumbleentertainment.com",
        "david.bethune@rumbleentertainment.com"
    };

    private static readonly string[] READONLY_DOMAINS =
    {
        "rumbleentertainment.com",
        "rumblegames.com",
        "willmaynard.com"
        // TODO: Testronic
    };

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
        {
            base.Add(output = Activator.CreateInstance<T>());
        }

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
                    group?.ConvertToAdmin();
                    break;
                case PassportType.Readonly:
                    group?.ConvertToReadonly();
                    break;
                case PassportType.Nonprivileged:
                    group?.ConvertToNonprivileged();
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
        {
            base.Add(toAdd);
        }
        else
        {
            current.Merge(toAdd);
        }
    }
    public enum PassportType { Superuser, Readonly, Nonprivileged, Unauthorized }

    public static Passport GetDefaultPermissions(SsoData data)
    {
        if (PlatformEnvironment.IsProd && PROD_SUPERUSERS.Contains(data.Email))
        {
            return new Passport(PassportType.Superuser);
        }

        if (!PlatformEnvironment.IsProd && DEV_SUPERUSERS.Contains(data.Email))
        {
            return new Passport(PassportType.Superuser);
        }

        if (READONLY_DOMAINS.Contains(data.Domain))
        {
            return new Passport(PassportType.Readonly);
        }

        // return new Passport(PassportType.Unauthorized);
        throw new PlatformException("Unauthorized.");
    }
}