using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;

namespace TowerPortal.Models.Player;

public class LinkedAccount : PlatformDataModel
{
    internal const string DB_KEY_CLIENT_VERSION = "clVer";
    internal const string DB_KEY_DATE_CREATED = "dtCreate";
    internal const string DB_KEY_DEVICE_TYPE = "dvcType";
    internal const string DB_KEY_LAST_SAVED_INSTALL_ID = "lastInstID";
    internal const string DB_KEY_SCREENNAME = "scrnname";
    internal const string DB_KEY_DISCRIMINATOR = "dsc";
    internal const string DB_KEY_USERNAME = "usrname";
    internal const string DB_KEY_ACCOUNT_ID_OVERRIDE = "accIdOver";
    internal const string DB_KEY_ID = "id";
    
    public const string FRIENDLY_KEY_CLIENT_VERSION = "clientVer";
    public const string FRIENDLY_KEY_DATE_CREATED = "dateCreated";
    public const string FRIENDLY_KEY_DEVICE_TYPE = "deviceType";
    public const string FRIENDLY_KEY_LAST_SAVED_INSTALL_ID = "lastSavedInstllID";
    public const string FRIENDLY_KEY_SCREENNAME = "screenname";
    public const string FRIENDLY_KEY_DISCRIMINATOR = "discriminator";
    public const string FRIENDLY_KEY_USERNAME = "username";
    public const string FRIENDLY_KEY_ACCOUNT_ID_OVERRIDE = "accIdOver";
    public const string FRIENDLY_KEY_ID = "id";
    
    [BsonElement(DB_KEY_CLIENT_VERSION)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_CLIENT_VERSION)]
    public string ClientVersion { get; set; }
    
    [BsonElement(DB_KEY_DATE_CREATED)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_DATE_CREATED)]
    public long DateCreated { get; set; }
    
    [BsonElement(DB_KEY_DEVICE_TYPE)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_DEVICE_TYPE)]
    public string DeviceType { get; set; }
    
    [BsonElement(DB_KEY_LAST_SAVED_INSTALL_ID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_LAST_SAVED_INSTALL_ID)]
    public string LastSavedInstallId { get; set; }
    
    [BsonElement(DB_KEY_SCREENNAME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_SCREENNAME)]
    public string Screenname { get; set; }
    
    [BsonElement(DB_KEY_DISCRIMINATOR)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_DISCRIMINATOR)]
    public int Discriminator { get; set; }
    
    [BsonElement(DB_KEY_USERNAME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_USERNAME)]
    public string Username { get; set; }
    
    [BsonElement(DB_KEY_ACCOUNT_ID_OVERRIDE)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ACCOUNT_ID_OVERRIDE)]
    public string AccountIdOverride { get; set; }
    
    [BsonElement(DB_KEY_ID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ID)]
    public string Id { get; set; }

    public LinkedAccount(string clientVersion, long dateCreated, string deviceType,
        string lastSavedInstallId, string screenname, int discriminator, string username, string accountIdOverride, string id)
    {
        ClientVersion = clientVersion;
        DateCreated = dateCreated;
        DeviceType = deviceType;
        LastSavedInstallId = lastSavedInstallId;
        Screenname = screenname;
        Discriminator = discriminator;
        Username = username;
        AccountIdOverride = accountIdOverride;
        Id = id;
    }
}