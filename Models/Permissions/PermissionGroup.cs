using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
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

    [BsonIgnore, JsonIgnore]
    internal string Key
    {
        get { return Name.Replace(oldChar: '.', newChar: '-').Replace(oldChar: ' ', newChar: '_'); }
    }

    internal string GetPermissionKey(string name)
    {
        return $"{Key}.{name}";
    }

    public Dictionary<string, bool> Values
    {
        get
        {
            Dictionary<string, bool> output = new Dictionary<string, bool>();
            foreach (PropertyInfo info in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(info => info.PropertyType.IsAssignableTo(typeof(bool))))
            {
                output[GetPermissionKey(info.Name)] = (bool)(info.GetValue(this) ?? false);
            }

            return output;
        }
    }

    public int UpdateFromValues(RumbleJson json)
    {
        int valuesChanged = 0;
        foreach (PropertyInfo info in GetType()
            .GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(prop => prop.PropertyType == typeof(bool)))
        {
            string key = GetPermissionKey(info.Name);
            bool? value = json.Optional<bool?>(key);
            if (value != null)
            {
                if (info.GetValue(this)?.Equals(value) ?? false)
                {
                    continue;
                }

                info.SetValue(this, (bool)value);
                valuesChanged++;
            }
            else
            {
                Log.Warn(Owner.Will, "Unable to update permission from RumbleJson; key not found.", data: new
                                                                                                           {
                                                                                                               PermissionKey = key
                                                                                                           });
            }
        }
        return valuesChanged;
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
            if (values.TryGetValue(group.Name + "." + info.Name, out bool permission))
            {
                info.SetValue(this, (bool)(info.GetValue(this) ?? false) || permission);
            }
        }
    }
}