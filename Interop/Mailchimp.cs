using System;
using RCL.Logging;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Interop;

public static class Mailchimp
{
    private const int RECORDS_PER_BATCH = 1_000;
    
    public static string MailchimpUrl => DynamicConfig.Instance?.Optional<string>("mailchimpUrl");
    public static string MailchimpListId => DynamicConfig.Instance?.Optional<string>("mailchimpListId");
    public static string MailchimpApiKey => DynamicConfig.Instance?.Optional<string>("mailchimpApiKey");

    // Resiliency for DC mistakes with double slashes
    private static string CleanUrl(string url) => url.Replace("//", "/").Replace(":/", "://");

    public static void PageThroughMembers(Action<MailchimpMember[]> members)
    {
        int processed = 0;
        int totalItems = 0;
        do
        {
            string url = CleanUrl($"{MailchimpUrl}/lists/{MailchimpListId}/members");
            
            RumbleJson payload = new()
            {
                { "count", RECORDS_PER_BATCH },
                { "offset", processed },
                {
                    "fields", string.Join(',', new[]
                    {
                        "total_items",
                        $"{MailchimpMember.KEY_PARENT_OBJECT}.{MailchimpMember.KEY_MEMBER_ID}",
                        $"{MailchimpMember.KEY_PARENT_OBJECT}.{MailchimpMember.KEY_EMAIL_ADDRESS}",
                        $"{MailchimpMember.KEY_PARENT_OBJECT}.{MailchimpMember.KEY_SUBSCRIPTION_STATUS}",
                        $"{MailchimpMember.KEY_PARENT_OBJECT}.{MailchimpMember.KEY_ADDITIONAL_DATA}",
                    })
                }
            };
            
            PlatformService
                .Optional<ApiService>()
                ?.Request(url)
                .AddHeader("Authorization", $"Basic {MailchimpApiKey}")
                .AddParameters(payload)
                .OnSuccess(response =>
                {
                    try
                    {
                        processed += RECORDS_PER_BATCH;
                        totalItems = response.Require<int>("total_items");
                        MailchimpMember[] results = response.Require<MailchimpMember[]>(MailchimpMember.KEY_PARENT_OBJECT);
                        MailchimpMember.AssignDownstreamValues(ref results);
                        members?.Invoke(results);
                    }
                    catch (Exception e)
                    {
                        Log.Error(Owner.Will, "Unable to batch Mailchimp members from a 200 HTTP code", exception: e);
                    }
                })
                .OnFailure(response =>
                {
                    Log.Error(Owner.Will, "Unable to fetch Mailchimp member information.", data: new
                    {
                        response = response,
                        processed = processed
                    });
                    totalItems = 0;
                })
                .Get();
        } while (processed < totalItems);
    }

    public static bool UpdateExistingMember(TokenInfo token, bool subscribed)
    {
        int code = 404;
        
        PlatformService
            .Optional<ApiService>()
            ?.Request(CleanUrl($"{MailchimpUrl}/lists/{MailchimpListId}/members/{token.Email}"))
            .SetRetries(0)
            .AddHeader("Authorization", $"Basic {MailchimpApiKey}")
            .SetPayload(new RumbleJson
            {
                { "status", (subscribed ? "subscribed" : "unsubscribed") },
                {
                    "merge_fields", new RumbleJson
                    {
                        { "FNAME", token.ScreenName },
                        { "PHONE", token.AccountId }
                    }
                }
            })
            .OnSuccess(response => Log.Local(Owner.Will, $"Successfully updated {token.Email}"))
            .OnFailure(response =>
            {
                Log.Local(Owner.Will, $"Unable to update Mailchimp member; {response.Optional<string>("detail") ?? "no detail provided"}");
                if (CreateNewSubscriber(token))
                    code = 200;
                else
                    Log.Error(Owner.Will, "Unable to update Mailchimp member", data: new
                    {
                        response = response
                    });
            })
            .Patch(out _, out code);

        return code.Between(200, 299);
    }

    public static bool CreateNewSubscriber(TokenInfo token)
    {
        int code = 404;
        
        PlatformService
            .Optional<ApiService>()
            ?.Request(CleanUrl($"{MailchimpUrl}/lists/{MailchimpListId}/members"))
            .AddHeader("Authorization", $"Basic {MailchimpApiKey}")
            .SetPayload(new RumbleJson
            {
                { MailchimpMember.KEY_EMAIL_ADDRESS, token.Email },
                { MailchimpMember.KEY_SUBSCRIPTION_STATUS, "subscribed" }
            })
            .OnSuccess(response => Log.Local(Owner.Will, $"Added {token.Email} as a Mailchimp subscriber!"))
            .OnFailure(response =>
            {
                Log.Error(Owner.Will, "Unable to create new Mailchimp subscriber", data: new
                {
                    response = response
                });
            })
            .Post(out _, out code);

        return code.Between(200, 299);
    }
}