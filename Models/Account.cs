using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

public class Account : PlatformCollectionDocument
{
    internal const string DB_KEY_NAME = "name";
    internal const string DB_KEY_EMAIL = "email";
    internal const string DB_KEY_ROLES = "roles";
    internal const string DB_KEY_FIRST_NAME = "fn";
    internal const string DB_KEY_LAST_NAME = "ln";

    public const string FRIENDLY_KEY_NAME = "name";
    public const string FRIENDLY_KEY_EMAIL = "email";
    public const string FRIENDLY_KEY_ROLES = "roles";
    public const string FRIENDLY_KEY_GIVEN_NAME = "givenName";
    public const string FRIENDLY_KEY_FAMILY_NAME = "familyName";

    [BsonElement(DB_KEY_NAME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_NAME)]
    public string Name => $"{FirstName} {FamilyName}";
    
    [BsonElement(DB_KEY_FIRST_NAME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_GIVEN_NAME), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string FirstName { get; private set; }
    [BsonElement(DB_KEY_LAST_NAME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_FAMILY_NAME), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string FamilyName { get; private set; }

    [BsonElement(DB_KEY_EMAIL)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_EMAIL)]
    public string Email { get; private set; }
    
    [BsonElement(DB_KEY_ROLES)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ROLES)]
    public List<string> Roles { get; private set; }

    public static Account FromGoogleClaims(IEnumerable<Claim> claims)
    {
        if (claims == null)
            return null;
        
        Account output = new Account();
        
        foreach (Claim claim in claims)
        {
            string type = claim.Type;

            if (type.EndsWith("/nameidentifier") || type.EndsWith("/name"))
                continue;
            
            if (type.EndsWith("/givenname"))
                output.FirstName = claim.Value.Trim();
            else if (type.EndsWith("/surname"))
                output.FamilyName = claim.Value.Trim();
            else if (type.EndsWith("/emailaddress"))
                output.Email = claim.Value.Trim();
        }
        
        return output;
    }
    
    // name
    // email
    // roles
}