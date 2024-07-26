using System.Text.Json.Serialization;
using Amazon.Auth.AccessControlPolicy;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities.JsonTools;

namespace Dmz.Models.Portal;

public class AuditLog : PlatformCollectionDocument
{
    private const string DB_KEY_ADDITIONAL_DATA = "extraData";
    private const string DB_KEY_DATA = "body";
    private const string DB_KEY_ENDPOINT = "url";
    private const string DB_KEY_MESSAGE = "msg";
    private const string DB_KEY_METHOD = "method";
    private const string DB_KEY_ACCOUNT_ID = "aid";
    private const string DB_KEY_HTTP_CODE = "code";
    private const string DB_KEY_RESPONSE = "response";
    private const string DB_KEY_TIMESTAMP = "ts";
    private const string DB_KEY_WHO = "who";

    public const string FRIENDLY_KEY_ADDITIONAL_DATA = "additionalData";
    public const string FRIENDLY_KEY_DATA = "requestData";
    public const string FRIENDLY_KEY_ENDPOINT = "endpoint";
    public const string FRIENDLY_KEY_MESSAGE = "message";
    public const string FRIENDLY_KEY_METHOD = "method";
    public const string FRIENDLY_KEY_ACCOUNT_ID = "accountId";
    public const string FRIENDLY_KEY_HTTP_CODE = "responseCode";
    public const string FRIENDLY_KEY_RESPONSE = "responseData";
    public const string FRIENDLY_KEY_TIMESTAMP = "timestamp";
    public const string FRIENDLY_KEY_WHO = "who";
    
    [BsonElement(DB_KEY_ADDITIONAL_DATA), BsonIgnoreIfNull]
    [JsonPropertyName(FRIENDLY_KEY_ADDITIONAL_DATA), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RumbleJson AdditionalData { get; set; }
    
    [BsonElement(DB_KEY_ENDPOINT)]
    [JsonPropertyName(FRIENDLY_KEY_ENDPOINT)]
    public string Endpoint { get; set; }
    
    [BsonElement(DB_KEY_MESSAGE), BsonIgnoreIfNull]
    [JsonPropertyName(FRIENDLY_KEY_MESSAGE), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Message { get; set; }
    
    [BsonElement(DB_KEY_METHOD)]
    [JsonPropertyName(FRIENDLY_KEY_METHOD)]
    public string Method { get; set; }
    
    [BsonElement(DB_KEY_DATA)]
    [JsonPropertyName(FRIENDLY_KEY_DATA), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RumbleJson Request { get; set; }
    
    [BsonElement(DB_KEY_RESPONSE), BsonIgnoreIfNull]
    [JsonPropertyName(FRIENDLY_KEY_RESPONSE), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RumbleJson Response { get; set; }
    
    [BsonElement(DB_KEY_HTTP_CODE)]
    [JsonPropertyName(FRIENDLY_KEY_HTTP_CODE)]
    public int ResultCode { get; set; }
    
    [BsonElement(DB_KEY_WHO)]
    [JsonPropertyName(FRIENDLY_KEY_WHO)]
    public TokenInfo Who { get; set; }

    public AuditLog SetCode(int code)
    {
        ResultCode = code;
        return this;
    }
}