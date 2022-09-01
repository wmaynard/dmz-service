using System.Text.Json.Serialization;
using Google.Apis.Auth;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;

namespace TowerPortal.Models.Portal;

public class SsoData : PlatformDataModel
{
    public const string FRIENDLY_KEY_ACCOUNT_ID = "accountId";
    public const string FRIENDLY_KEY_DOMAIN = "domain";
    public const string FRIENDLY_KEY_EMAIL = "email";
    public const string FRIENDLY_KEY_NAME = "name";
    public const string FRIENDLY_KEY_PHOTO = "photo";

    [BsonIgnore]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ACCOUNT_ID), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string AccountId { get; init; }

    [BsonIgnore]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_EMAIL), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Email { get; init; }
    
    [BsonIgnore]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_NAME), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; init; }

    [BsonIgnore]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_PHOTO), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Photo { get; init; }
    
    [BsonIgnore]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_DOMAIN), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Domain { get; init; }

    // ReSharper disable once ArrangeMethodOrOperatorBody
    public static implicit operator SsoData(GoogleJsonWebSignature.Payload payload) => new SsoData
                                                                                       {
                                                                                           AccountId = payload.Subject,
                                                                                           Email = payload.Email,
                                                                                           Photo = payload.Picture,
                                                                                           Name = payload.Name,
                                                                                           Domain = payload.HostedDomain
                                                                                       };
}