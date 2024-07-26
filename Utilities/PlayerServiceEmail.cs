using System;
using Dmz.Interop;
using Dmz.Models;
using Dmz.Services;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Utilities.JsonTools;

namespace Dmz.Utilities;

public static class PlayerServiceEmail
{
    public const long DELAY_WELCOME_EMAIL_DEFAULT = 24 * 60 * 60; // one day in seconds
    public const string CONFIG_WELCOME_DELAY = "delayWelcomeEmail";
    public static readonly string TEMPLATE_CONFIRMATION = $"{PlatformEnvironment.Deployment}-tower-account-confirmation";
    
    // public const string TEMPLATE_CONFIRMATION = "107-tower-account-confirmation";
    public static readonly string TEMPLATE_2FA = $"{PlatformEnvironment.Deployment}-tower-login-2fa";
    public static readonly string TEMPLATE_PASSWORD_RESET = $"{PlatformEnvironment.Deployment}-tower-reset-password";
    public static readonly string TEMPLATE_NEW_DEVICE_NOTIFICATION = $"{PlatformEnvironment.Deployment}-tower-new-device-notification";
    public static readonly string TEMPLATE_WELCOME = $"{PlatformEnvironment.Deployment}-tower-welcome";

    public static void SendConfirmation(string email, string accountId, string code, long expiration) =>
        AmazonSes.SendEmail(email, TEMPLATE_CONFIRMATION, replacements: new RumbleJson
        {
            { "endpoint", PlatformEnvironment.Url("/dmz/player/account/confirm") },
            { "accountId", accountId },
            { "code", code },
            { "duration", TimestampToText(expiration) }
        });

    public static void SendPasswordReset(string email, string accountId, string code,  long expiration) =>
        AmazonSes.SendEmail(email, TEMPLATE_PASSWORD_RESET, replacements: new RumbleJson
        {
            { "endpoint", PlatformEnvironment.Url("/dmz/player/account/reset") },
            { "accountId", accountId },
            { "code", code },
            { "duration", TimestampToText(expiration) }
        });

    public static void SendNewDeviceLogin(string email, string device) =>
        AmazonSes.SendEmail(email, TEMPLATE_NEW_DEVICE_NOTIFICATION, replacements: new RumbleJson
        {
            { "device", device },
            { "timestamp", TimestampToDate() }
        });
    
    public static void SendTwoFactorCode(string email, string code, long expiration) =>
        AmazonSes.SendEmail(email, TEMPLATE_2FA, replacements: new RumbleJson
        {
            { "code", code },
            { "duration", TimestampToText(expiration) }
        });

    public static void SendWelcome(string email) =>
        AmazonSes.SendEmail(email, TEMPLATE_WELCOME, replacements: new RumbleJson
        {
            
        }, canUnsub: true);

    public static ScheduledEmail ScheduleWelcome(string email)
    {
        long delay = DynamicConfig
             .Instance
             ?.GetValuesFor(Audience.PlayerService)
             ?.Optional<long?>(CONFIG_WELCOME_DELAY)
            ?? DELAY_WELCOME_EMAIL_DEFAULT;
        
        return AmazonSes.CraftScheduledEmail(address: email, delay, templateName: TEMPLATE_WELCOME);
    }

    private static string TimestampToText(long expiration)
    {
        TimeSpan time = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: (int)(expiration - Timestamp.Now));

        int hours = (int)time.TotalDays * 24 + time.Hours;
        int minutes = time.Minutes + (time.Seconds > 0 ? 1 : 0); // round seconds up

        while (minutes > 60)
        {
            hours++;
            minutes -= 60;
        }

        string output = "";

        if (hours > 0)
            output += $" {hours} hours";
        if (minutes > 0)
            output += $" {minutes} minutes";
        
        return output.Trim();
    }

    private static string TimestampToDate(long? timestamp = null) => DateTimeOffset
        .FromUnixTimeSeconds(timestamp ?? Timestamp.Now)
        .DateTime
        .ToString(format: "MM/dd/yyyy HH:mm (UTC)");
}