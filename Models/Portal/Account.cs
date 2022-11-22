using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Dmz.Models.Permissions;
using MongoDB.Bson.Serialization.Attributes;
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
    internal const string DB_KEY_NAME        = "name";
    internal const string DB_KEY_EMAIL       = "email";
    internal const string DB_KEY_ROLES       = "roles";
    internal const string DB_KEY_PERMISSIONS = "perms";
    internal const string DB_KEY_FIRST_NAME  = "fn";
    internal const string DB_KEY_LAST_NAME   = "ln";

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
    public string FirstName
    {
        get
        {
            try
            {
                return Name.Split(separator: ' ').First();
            }
            catch
            {
                return null;
            }
        }
    }
    
    [BsonIgnore]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_FAMILY_NAME), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string FamilyName
    {
        get
        {
            try
            {
                return Name.Split(separator: ' ').Last();
            }
            catch
            {
                return null;
            }
        }
    }

    [BsonElement(DB_KEY_EMAIL)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_EMAIL)]
    public string Email { get; private set; }
    
    [BsonElement(DB_KEY_ROLES)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ROLES)]
    public List<Role> Roles { get; set; }
    
    [BsonElement(DB_KEY_PERMISSIONS)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_PERMISSIONS)]
    public Passport Permissions { get; set; } // TODO: private set

    public Account() => Permissions = new Passport();

    public static Account FromSsoData(SsoData data) => new Account
                                                       {
                                                           Name = data.Name,
                                                           Email = data.Email,
                                                           Permissions = Passport.GetDefaultPermissions(data)
                                                       };

    // Temporary to add in missing properties
    public void InitPropertyRole()
    {
        Roles = new List<Role>();
    }
}