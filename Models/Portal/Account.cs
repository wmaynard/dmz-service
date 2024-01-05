using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Dmz.Models.Permissions;
using Dmz.Services;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Data;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeAttributes
// ReSharper disable ArrangeConstructorOrDestructorBody
// ReSharper disable ArrangeMethodOrOperatorBody

namespace Dmz.Models.Portal;

[BsonIgnoreExtraElements]
public class Account : PlatformCollectionDocument
{
    internal const int MAX_ACTIVITY_LOG_STORAGE = 1_000;
    internal const string INDEX_ACTIVITY = "activityLog";
    
    internal const string DB_KEY_ACTIVITY    = "log";
    internal const string DB_KEY_NAME        = "name";
    internal const string DB_KEY_EMAIL       = "email";
    internal const string DB_KEY_ROLES       = "roles";
    internal const string DB_KEY_PERMISSIONS = "perms";
    internal const string DB_KEY_FIRST_NAME  = "fn";
    internal const string DB_KEY_LAST_NAME   = "ln";

    public const string FRIENDLY_KEY_ACTIVITY    = "activity";
    public const string FRIENDLY_KEY_NAME        = "name";
    public const string FRIENDLY_KEY_EMAIL       = "email";
    public const string FRIENDLY_KEY_ROLES       = "roles";
    public const string FRIENDLY_KEY_PERMISSIONS = "permissions";
    public const string FRIENDLY_KEY_GIVEN_NAME  = "givenName";
    public const string FRIENDLY_KEY_FAMILY_NAME = "familyName";

    [BsonElement(DB_KEY_NAME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_NAME)]
    public string Name { get; set; }

    [BsonIgnore]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_GIVEN_NAME), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string FirstName => Name?.Split(separator: ' ').First();
    
    [BsonIgnore]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_FAMILY_NAME), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string FamilyName => Name?.Split(separator: ' ').Last();

    [BsonElement(DB_KEY_EMAIL)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_EMAIL)]
    public string Email { get; set; }
    
    [BsonElement(DB_KEY_ROLES)]
    [JsonInclude, JsonPropertyName("roleIds")]
    public string[] RoleIds { get; set; }
    
    [BsonIgnore]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ROLES)]
    public Role[] Roles { get; set; }
    
    [BsonElement(DB_KEY_PERMISSIONS)]
    [JsonIgnore]
    public Passport Permissions { get; set; }

    // Simplify permissions for output; due to a janky way mongo tries to serialize Passports, we have to do some dumb workarounds.
    [BsonIgnore]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_PERMISSIONS)]
    public Dictionary<string, bool> Perms => Permissions
        .SelectMany(group => group.Values)
        .ToDictionary(
            keySelector: pair => pair.Key,
            elementSelector: pair => pair.Value
        );

    public static Account FromSsoData(SsoData data) => new()
   {
       Name = data.Name,
       Email = data.Email,
       Permissions = Passport.GetDefaultPermissions(data)
   };

    public void LoadPermissionsFrom(Role[] roles)
    {
        foreach (Role role in roles)
            Permissions.Merge(role.Permissions);
    }
}