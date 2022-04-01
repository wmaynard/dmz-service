using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Models;

public class SearchPlayer : PlatformDataModel
{
    internal const string DB_KEY_CLIENT_VERSION = "clVer";
    internal const string DB_KEY_DATE_CREATED = "dtCreate";
    internal const string DB_KEY_DATA_VERSION = "dataVer";
    internal const string DB_KEY_DEVICE_TYPE = "dvcType";
    internal const string DB_KEY_LAST_SAVED_INSTALL_ID = "lastInstID";
    internal const string DB_KEY_MERGE_VERSION = "mrgVer";
    internal const string DB_KEY_LAST_CHANGED = "lastChange";
    internal const string DB_KEY_LAST_DATA_VERSION = "lastDataVer";
    internal const string DB_KEY_SCREENNAME = "scrnName";
    internal const string DB_KEY_LAST_UPDATED = "lastUpdate";
    internal const string DB_KEY_DISCRIMINATOR = "dsc";
    internal const string DB_KEY_USERNAME = "usrName";
    internal const string DB_KEY_SEARCH_WEIGHT = "srchWeight";
    internal const string DB_KEY_ID = "id";
    
    public const string FRIENDLY_KEY_CLIENT_VERSION = "clientVer";
    public const string FRIENDLY_KEY_DATE_CREATED = "dateCreated";
    public const string FRIENDLY_KEY_DATA_VERSION = "dataVersion";
    public const string FRIENDLY_KEY_DEVICE_TYPE = "deviceType";
    public const string FRIENDLY_KEY_LAST_SAVED_INSTALL_ID = "lastSavedInstllID";
    public const string FRIENDLY_KEY_MERGE_VERSION = "mergeVersion";
    public const string FRIENDLY_KEY_LAST_CHANGED = "lastChanged";
    public const string FRIENDLY_KEY_LAST_DATA_VERSION = "lastDataVersion";
    public const string FRIENDLY_KEY_SCREENNAME = "screenName";
    public const string FRIENDLY_KEY_LAST_UPDATED = "lastUpdated";
    public const string FRIENDLY_KEY_DISCRIMINATOR = "discriminator";
    public const string FRIENDLY_KEY_USERNAME = "userName";
    public const string FRIENDLY_KEY_SEARCH_WEIGHT = "searchWeight";
    public const string FRIENDLY_KEY_ID = "id";
    
    [BsonElement(DB_KEY_CLIENT_VERSION)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_CLIENT_VERSION)]
    public string ClientVersion { get; private set; }
    
    [BsonElement(DB_KEY_DATE_CREATED)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_DATE_CREATED)]
    public long DateCreated { get; private set; }
    
    [BsonElement(DB_KEY_DATA_VERSION)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_DATA_VERSION)]
    public string DataVersion { get; private set; }
    
    [BsonElement(DB_KEY_DEVICE_TYPE)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_DEVICE_TYPE)]
    public string DeviceType { get; private set; }
    
    [BsonElement(DB_KEY_LAST_SAVED_INSTALL_ID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_LAST_SAVED_INSTALL_ID)]
    public string LastSavedInstallId { get; private set; }
    
    [BsonElement(DB_KEY_MERGE_VERSION)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_MERGE_VERSION)]
    public string MergeVersion { get; private set; }
    
    [BsonElement(DB_KEY_LAST_CHANGED)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_LAST_CHANGED)]
    public long LastChanged { get; private set; }
    
    [BsonElement(DB_KEY_LAST_DATA_VERSION)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_LAST_DATA_VERSION)]
    public string LastDataVersion { get; private set; }
    
    [BsonElement(DB_KEY_SCREENNAME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_SCREENNAME)]
    public string Screenname { get; private set; }
    
    [BsonElement(DB_KEY_LAST_UPDATED)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_LAST_UPDATED)]
    public long LastUpdated { get; private set; }
    
    [BsonElement(DB_KEY_DISCRIMINATOR)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_DISCRIMINATOR)]
    public int Discriminator { get; private set; }
    
    [BsonElement(DB_KEY_USERNAME)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_USERNAME)]
    public string Username { get; private set; }
    
    [BsonElement(DB_KEY_SEARCH_WEIGHT)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_SEARCH_WEIGHT)]
    public int SearchWeight { get; private set; }
    
    [BsonElement(DB_KEY_ID)]
    [JsonInclude, JsonPropertyName(FRIENDLY_KEY_ID)]
    public string Id { get; private set; }

    public SearchPlayer(string clientVersion, long dateCreated, string dataVersion, string deviceType,
        string lastSavedInstallId, string mergeVersion, long lastChanged, string lastDataVersion, string screenname,
        long lastUpdated, int discriminator, string username, int searchWeight, string id)
    {
        ClientVersion = clientVersion;
        DateCreated = dateCreated;
        DataVersion = dataVersion;
        DeviceType = deviceType;
        LastSavedInstallId = lastSavedInstallId;
        MergeVersion = mergeVersion;
        LastChanged = lastChanged;
        LastDataVersion = lastDataVersion;
        LastUpdated = lastUpdated;
        Discriminator = discriminator;
        Username = username;
        SearchWeight = searchWeight;
        Id = id;
    }
}