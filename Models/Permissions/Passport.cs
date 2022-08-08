using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Exceptions;

namespace TowerPortal.Models.Permissions;

public class Passport : List<PermissionGroup>
{
    public ConfigPermissions Config => Fetch<ConfigPermissions>();
    public LeaderboardsPermissions Leaderboard => Fetch<LeaderboardsPermissions>();
    public MailPermissions Mail => Fetch<MailPermissions>();
    public MatchmakingPermissions Matchmaking => Fetch<MatchmakingPermissions>();
    public MultiplayerPermissions Multiplayer => Fetch<MultiplayerPermissions>();
    public NftPermissions Nft => Fetch<NftPermissions>();
    public ReceiptPermissions Receipt => Fetch<ReceiptPermissions>();
    public PlayerPermissions Player => Fetch<PlayerPermissions>();
    public PortalPermissions Portal => Fetch<PortalPermissions>();
    public PvpPermissions Pvp => Fetch<PvpPermissions>();
    public TokenPermissions Token => Fetch<TokenPermissions>();

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
                    throw new PlatformException(message: "Unauthorized user permissions instantiated.");
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
}