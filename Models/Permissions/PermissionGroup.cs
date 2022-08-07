using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;
using RCL.Logging;
using Rumble.Platform.Common.Exceptions;

namespace TowerPortal.Models.Permissions;

public class Passport : List<PermissionGroup>
{
    public ConfigServicePermissions Config => Fetch<ConfigServicePermissions>();
    public MailServicePermissions Mail => Fetch<MailServicePermissions>();
    public PlayerServicePermissions Player => Fetch<PlayerServicePermissions>();
    public PlayerServicePermissions Token => Fetch<PlayerServicePermissions>();
    public PortalPermissions Portal => Fetch<PortalPermissions>();

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

public abstract class PermissionGroup
{
    /// <summary>
    /// Used to display the group in an organized way when managing permissions.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Able to view the tab / button to access the page.
    /// </summary>
    public bool View_Navbar { get; init; }
    
    /// <summary>
    /// Able to view the actual landing page itself.  If false, the user should be redirected to a 403 Forbidden page.
    /// </summary>
    public bool View_Page { get; set; }
    
    [Obsolete("Anything relying on the Edit permission needs to be transitioned to more specific values.")]
    public bool Edit { get; set; }

    public Dictionary<string, bool> Values
    {
        get
        {
            Dictionary<string, bool> output = new Dictionary<string, bool>();
            foreach (PropertyInfo info in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(info => info.PropertyType.IsAssignableTo(typeof(bool))))
                output[info.Name] = (bool)(info.GetValue(this) ?? false);
            return output;
        }
    }
    
    public void ConvertToAdmin()
    {
        foreach (PropertyInfo info in GetType()
                     .GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .Where(info => info.PropertyType.IsAssignableTo(typeof(bool))))
        {
            info.SetValue(obj: this, value: true);
        }
    }
    
    public void ConvertToReadonly()
    {
        foreach (PropertyInfo info in GetType()
                     .GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .Where(info => info.PropertyType.IsAssignableTo(typeof(bool)))
                     .Where(info => info.Name.Contains("View_")))
        {
            info.SetValue(obj: this, value: true);
        }
    }
    public void ConvertToNonprivileged()
    {
        foreach (PropertyInfo info in GetType()
                     .GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .Where(info => info.PropertyType.IsAssignableTo(typeof(bool))))
        {
            info.SetValue(obj: this, value: false);
        }
    }

    // TODO: Test this
    public void Merge(PermissionGroup group)
    {
        Dictionary<string, bool> values = group.Values;
        foreach (PropertyInfo info in GetType()
                     .GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .Where(info => info.PropertyType.IsAssignableTo(typeof(bool))))
        {
            if (values.TryGetValue(info.Name, out bool permission))
                info.SetValue(this, (bool)(info.GetValue(this) ?? false) || permission);
        }
    }
}

public class PlayerServicePermissions : PermissionGroup
{
    public override string Name => "Player Service";
    
    // TODO: Add permissions
}

public class MailServicePermissions : PermissionGroup
{
    public override string Name => "Mail Service";

    public bool SendDirectMessages { get; set; }
    public bool SendGlobalMessages { get; set; }
}

public class TokenServicePermissions : PermissionGroup
{
    public override string Name => "Token Service";
}

public class ConfigServicePermissions : PermissionGroup
{
    public override string Name => "Config Service";
}

public class PortalPermissions : PermissionGroup
{
    public override string Name => "Portal";
    
    public bool SuperUser { get; set; }
    public bool ManagePermissions { get; set; }
}