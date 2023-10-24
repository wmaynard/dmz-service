using System;
using System.Collections.Generic;
using System.Linq;
using RCL.Logging;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Minq;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Interop;

public class MailchimpService : MinqTimerService<MailchimpMember>
{
    private readonly DynamicConfig _config;
    // TODO: Move this updating functionality to a QueueService so that only one node per environment is running it.  Then lower the interval.
    public MailchimpService(DynamicConfig config) : base("subscribers", 360_000) => _config = config;

    protected override void OnElapsed()
    {
        Mailchimp.PageThroughMembers(members =>
        {
            string[] subscribers = members
                .Where(member => member.SubscriptionStatus == MailchimpSubscriptionStatus.Subscribed)
                .Select(member => member.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .ToArray();
            string[] nonsubscribers = members
                .Where(member => member.SubscriptionStatus == MailchimpSubscriptionStatus.Unsubscribed)
                .Select(member => member.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .ToArray();

            long subbed = 0;
            long unsubbed = 0;

            mongo.WithTransaction(out Transaction transaction);

            if (subscribers.Any())
                subbed = mongo
                    .WithTransaction(out transaction)
                    .Where(query => query.ContainedIn(member => member.Email, subscribers))
                    .Update(query => query
                        .Set(member => member.Status, "subscribed")
                    );
            if (nonsubscribers.Any())
                unsubbed = mongo
                    .WithTransaction(transaction)
                    .Where(query => query.ContainedIn(member => member.Email, nonsubscribers))
                    .Update(query => query
                        .Set(member => member.Status, "unsubscribed")
                    );

            transaction.Commit();
            
            Log.Local(Owner.Will, $"Affected {subbed + unsubbed} records.");
            
            if (subbed + unsubbed > 0)
                Log.Info(Owner.Will, "Registered changed un/subbed Mailchimp statuses", data: new
                {
                    newSubscribers = subbed,
                    newUnsubscribers = unsubbed
                });
        });
    }

    public MailchimpMember GetMember(string accountId) => mongo
        .Where(query => query.EqualTo(member => member.AccountId, accountId))
        .Upsert(query => query
            .SetOnInsert(member => member.Status, "unsubscribed")
            .SetOnInsert(member => member.CreatedOn, Timestamp.UnixTime)
        );

    public void ClaimEmail(string accountId, string templateId)
    {
        if (string.IsNullOrWhiteSpace(templateId))
            throw new PlatformException("Invalid Mailchimp template ID.");
        
        string[] rewardCsv = _config.Optional<string>($"mailchimpReward_{templateId}")?.Split(',');
        
        // Expected format: subject,body,type,rewardId,quantity
        // The last three can repeat, but always need to be a group of 3
        if (rewardCsv == null || rewardCsv.Length < 8 || (rewardCsv.Length - 5) % 3 != 0)
            throw new PlatformException("Mailchimp reward not configured properly.");

        string title = rewardCsv[0];
        string body = rewardCsv[1];
        string icon = rewardCsv[2];
        string banner = rewardCsv[3];
        string note = rewardCsv[4];

        List<RumbleJson> attachments = new List<RumbleJson>();
        for (int index = 5; index < rewardCsv.Length; index += 3)
            attachments.Add(new RumbleJson
            {
                { "type", rewardCsv[index] },
                { "rewardId", rewardCsv[index + 1] },
                { "quantity", rewardCsv[index + 2] }
            });
        
        long affected = mongo
            .WithTransaction(out Transaction transaction)
            .Where(query => query.EqualTo(member => member.AccountId, accountId))
            .Update(query => query.Union(member => member.ClaimedEmails, templateId));

        if (affected == 0)
            throw new PlatformException("Already claimed.", code: ErrorCode.MongoUnexpectedAffectedCount);

        int code = 404;
        ApiService
            .Instance
            ?.Request("/mail/admin/messages/send")
            .AddAuthorization(DynamicConfig.Instance?.AdminToken)
            .SetPayload(new RumbleJson
            {
                { "accountIds", new[] { accountId } },
                {
                    "message", new RumbleJson
                    {
                        { "subject", title },
                        { "body", body },
                        { "attachments", attachments },
                        { "expiration", Timestamp.InTheFuture(days: 7) },
                        { "visibleFrom", Timestamp.UnixTime },
                        { "icon", icon },
                        { "banner", banner },
                        { "internalNote", note }
                    }
                }
            })
            .OnFailure(response => Log.Error(Owner.Will, "Unable to send Mailchimp reward.", data: new
            {
                response = response
            }))
            .Post(out _, out code);

        if (!code.Between(200, 299))
        {
            transaction.Abort();
            throw new PlatformException("Unable to send mail.", code: ErrorCode.ApiFailure);
        }

        transaction.Commit();
    }

    public void AlterSubscription(TokenInfo player, bool isSubscribed)
    {
        EnforceEmailExists(player);
        
        mongo
            .WithTransaction(out Transaction transaction)
            .Where(query => query.EqualTo(member => member.AccountId, player.AccountId))
            .Upsert(query => query
                .Set(member => member.Status, isSubscribed ? "subscribed" : "unsubscribed")
                .Set(member => member.AccountId, player.AccountId)
                .Set(member => member.Screenname, player.ScreenName)
                .Set(member => member.Email, player.Email)
                .SetOnInsert(member => member.CreatedOn, Timestamp.UnixTime)
            );
        
        if (Mailchimp.UpdateExistingMember(player, isSubscribed))
            transaction.Commit();
        else
        {
            transaction.Abort();
            throw new PlatformException("Unable to update existing member on Mailchimp's service.", code: ErrorCode.ApiFailure);
        }
    }

    private bool EnforceEmailExists(TokenInfo token) => EmailRegex.IsValid(token.Email)
        ? true
        : throw new PlatformException("Email unavailable in token; Mailchimp requires an email to unsubscribe via API", code: ErrorCode.InvalidRequestData);

    public override long ProcessGdprRequest(TokenInfo token, string dummyText) => mongo
        .Where(query => query.EqualTo(member => member.AccountId, token?.AccountId))
        .Or(query => query.EqualTo(member => member.Email, token?.Email))
        .Update(query => query.Set(member => member.Email, dummyText));
}