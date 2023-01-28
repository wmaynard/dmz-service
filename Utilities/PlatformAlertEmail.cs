using Dmz.Interop;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Models.Alerting;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Utilities;

public static class PlatformAlertEmail
{
    public static readonly string TEMPLATE_ALERT = $"{PlatformEnvironment.Deployment}-rumble-log-alert";

    public static void SendAlert(string email, Alert alert) =>
        AmazonSes.SendEmail(email, TEMPLATE_ALERT, replacements: new RumbleJson
        {
            { "title", alert.Title },
            { "message", alert.Message },
            { "id", alert.Id },
            { "audience", $"{alert.Origin} ({PlatformEnvironment.Deployment})" },
            { "owner", alert.Owner.GetDisplayName() },
            { "timeActive", (Timestamp.UnixTime - alert.CreatedOn).ToFriendlyTime() },
            { "status", alert.Status.GetDisplayName() },
            { "impact", alert.Impact.GetDisplayName() },
            { "count", alert.Trigger.Count },
            { "period", alert.Trigger.Timeframe.ToFriendlyTime() },
            { "acknowledge", PlatformEnvironment.Url("alert/acknowledge") },
            { "resolve", PlatformEnvironment.Url("alert/resolve") },
            { "cancel", PlatformEnvironment.Url("alert/cancel") }
            
        }).Wait();
}