using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MongoDB.Bson.Serialization.Attributes;
using RCL.Logging;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeAttributes

namespace Dmz.Models.Permissions;

[BsonDiscriminator(Required = true)]
[BsonIgnoreExtraElements]
public abstract class PermissionGroup : PlatformDataModel
{
    /// <summary>
    /// Used to display the group in an organized way when managing permissions.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Able to view the actual landing page itself.  If false, the user should be redirected to a 403 Forbidden page.
    /// </summary>
    public bool View { get; set; }

    private PropertyInfo[] BooleanProperties => GetType()
        .GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Where(info => info.PropertyType.IsAssignableTo(typeof(bool)))
        .ToArray();

    [BsonIgnore, JsonIgnore]
    internal string Key => Name
        .Replace(oldChar: '.', newChar: '-')
        .Replace(oldChar: ' ', newChar: '_');

    internal string GetPermissionKey(string name) => $"{Key}.{name}";

    public Dictionary<string, bool> Values => BooleanProperties.ToDictionary(
        keySelector: prop => GetPermissionKey(prop.Name),
        elementSelector: prop => (bool)(prop.GetValue(this) ?? false)
    );


    public int UpdateFromValues(RumbleJson json)
    {
        int valuesChanged = 0;
        foreach (PropertyInfo info in BooleanProperties)
        {
            string key = GetPermissionKey(info.Name);
            bool? value = json.Optional<bool?>(key);

            if (value == null)                                  // Request didn't specify this value; move on.
                continue;
            
            if (info.GetValue(this)?.Equals(value) ?? false)    // Request value is the same; move on.
                continue;

            info.SetValue(this, (bool)value);
            valuesChanged++;
        }
        return valuesChanged;
    }

    public void UpdatePermissions(bool value, string filter = null)
    {
        PropertyInfo[] perms = string.IsNullOrWhiteSpace(filter)
            ? BooleanProperties
            : BooleanProperties
                .Where(prop => prop.Name.Contains(filter))
                .ToArray();
        
        foreach(PropertyInfo info in perms)
            info.SetValue(this, value);
    }

    // TODO: Test this
    public void Merge(PermissionGroup group)
    {
        if (group == null)
            return;
        foreach (PropertyInfo info in BooleanProperties)
            if (group.Values.TryGetValue(GetPermissionKey(info.Name), out bool permission)) // look up the parameter group's permission
            {
                if (group.Name.Contains("Chat"))
                {
                    Console.Write("");
                }
                info.SetValue(this, (bool) (info.GetValue(this) ?? false) || permission);
            }
    }
}