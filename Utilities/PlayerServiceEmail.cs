using System;
using Dmz.Interop;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Utilities;

public static class PlayerServiceEmail
{
    public static readonly string TEMPLATE_CONFIRMATION = $"{PlatformEnvironment.Deployment}-tower-account-confirmation";
    
    // public const string TEMPLATE_CONFIRMATION = "107-tower-account-confirmation";
    public static readonly string TEMPLATE_2FA = $"{PlatformEnvironment.Deployment}-tower-login-2fa";
    public static readonly string TEMPLATE_PASSWORD_RESET = $"{PlatformEnvironment.Deployment}-tower-reset-password";
    public static readonly string TEMPLATE_NEW_DEVICE_NOTIFICATION = $"{PlatformEnvironment.Deployment}-tower-new-device-notification";

    public static void SendConfirmation(string email, string accountId, string code, long expiration) =>
        AmazonSes.SendEmail(email, TEMPLATE_CONFIRMATION, replacements: new RumbleJson
        {
            { "endpoint", PlatformEnvironment.Url("/dmz/player/account/confirm") },
            { "accountId", accountId },
            { "code", code },
            { "duration", TimestampToText(expiration) }
        }).Wait();

    public static void SendPasswordReset(string email, string accountId, string code,  long expiration) =>
        AmazonSes.SendEmail(email, TEMPLATE_PASSWORD_RESET, replacements: new RumbleJson
        {
            { "endpoint", PlatformEnvironment.Url("/dmz/player/account/reset") },
            { "accountId", accountId },
            { "code", code },
            { "duration", TimestampToText(expiration) }
        }).Wait();

    public static void SendNewDeviceLogin(string email, string device) =>
        AmazonSes.SendEmail(email, TEMPLATE_NEW_DEVICE_NOTIFICATION, replacements: new RumbleJson
        {
            { "device", device },
            { "timestamp", TimestampToDate() }
        }).Wait();
    
    public static void SendTwoFactorCode(string email, string code, long expiration) =>
        AmazonSes.SendEmail(email, TEMPLATE_2FA, replacements: new RumbleJson
        {
            { "code", code },
            { "duration", TimestampToText(expiration) }
        }).Wait();

    private static string TimestampToText(long expiration)
    {
        TimeSpan time = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: (int)(expiration - Timestamp.UnixTime));

        int hours = (int)time.TotalDays * 24 + time.Hours;
        int minutes = time.Minutes + (time.Seconds > 0 ? 1 : 0); // round seconds up

        string output = "";

        if (hours > 0)
            output += $" {hours} hours";
        if (minutes > 0)
            output += $" {minutes} minutes";
        
        return output.Trim();
    }

    private static string TimestampToDate(long? timestamp = null) => DateTimeOffset
        .FromUnixTimeSeconds(timestamp ?? Timestamp.UnixTime)
        .DateTime
        .ToString(format: "yyyy.MM.dd HH:mm:ss UTC");
}