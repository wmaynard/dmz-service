using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using TowerPortal.Enums;

namespace TowerPortal.Utilities;

public static class TempDataExtensions
{
    private const string KEY_STATUS_MESSAGE = "StatusMessage";
    private const string KEY_STATUS = "Status";
    
    public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
    {
        tempData[key] = JsonConvert.SerializeObject(value);
    }

    public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
    {
        object o;
        tempData.TryGetValue(key, out o);
        return o == null ? null : JsonConvert.DeserializeObject<T>((string)o);
    }
    
    public static void SetStatusMessage(this ITempDataDictionary data, string message, RequestStatus status)
    {
        data[KEY_STATUS_MESSAGE] = message;
        data[KEY_STATUS] = status;
    }

    public static string GetStatusMessage(this ITempDataDictionary data) => (string)data[KEY_STATUS_MESSAGE];

    public static RequestStatus GetStatus(this ITempDataDictionary data) => data.ContainsKey(KEY_STATUS)
        ? (RequestStatus)data[KEY_STATUS]
        : RequestStatus.None;

    public static bool WasSuccessful(this ITempDataDictionary data) => data.GetStatus() == RequestStatus.Success;
}