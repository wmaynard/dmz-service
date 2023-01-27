using Dmz.Interop;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Utilities;

public static class PlatformAlertEmail
{
    public static readonly string TEMPLATE_ALERT = $"{PlatformEnvironment.Deployment}-rumble-log-alert";

    public static void SendAlert(string email, string title, string message, string id, string audience, string owner, long timestamp, string status, string impact) =>
        AmazonSes.SendEmail(email, TEMPLATE_ALERT, replacements: new RumbleJson
        {
            { "title", title },
            { "message", message },
            { "id", id },
            { "audience", audience },
            { "owner", owner },
            { "timeActive", (Timestamp.UnixTime - timestamp).ToFriendlyTime() },
            { "status", status },
            { "impact", impact },
            { "acknowledge", PlatformEnvironment.Url("alert/acknowledge") },
            { "resolve", PlatformEnvironment.Url("alert/resolve") },
            { "cancel", PlatformEnvironment.Url("alert/cancel") }
            
        }).Wait();
}