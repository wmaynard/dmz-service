using System;
using System.Threading.Tasks;
using Dmz.Interop;
using Dmz.Models;
using MongoDB.Driver;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Services;

public class ScheduledEmailService : PlatformMongoTimerService<ScheduledEmail>
{
    private const long SENT_RETENTION_SECONDS = 30 * 24 * 60 * 60;
    private const int MAX_ATTEMPTS = 5;
    public ScheduledEmailService() : base(collection: "outbox", intervalMs: 30_000, startImmediately: true) { }

    protected override void OnElapsed()
    {
        // Attempt to send up to 100 emails per elapsed tick.
        for (int sent = 0; sent < 100 && TrySendEmail(); sent++);

        _collection.DeleteMany(
            filter: Builders<ScheduledEmail>.Filter.And(
                Builders<ScheduledEmail>.Filter.Eq(email => email.Sent, true),
                Builders<ScheduledEmail>.Filter.Lt(email => email.SentOn, Timestamp.Now - SENT_RETENTION_SECONDS)
            )
        );
    }

    /// <summary>
    /// Finds an email that has not yet been sent, marks it as sent so it doesn't get claimed by another container.
    /// If the email sending fails, the email is reset to unsent status.  If the email fails more than 5 times it won't
    /// be retried.
    /// </summary>
    /// <returns>True if an email was found and sending was attempted.  Otherwise, false.</returns>
    private bool TrySendEmail()
    {
        ScheduledEmail toSend = _collection.FindOneAndUpdate(
            filter: Builders<ScheduledEmail>.Filter.And(
                Builders<ScheduledEmail>.Filter.Eq(email => email.Sent, false),
                Builders<ScheduledEmail>.Filter.Lt(email => email.SendAfter, Timestamp.Now),
                Builders<ScheduledEmail>.Filter.Lt(email => email.Attempts, MAX_ATTEMPTS)
            ),
            update: Builders<ScheduledEmail>.Update
                .Set(email => email.Sent, true)
                .Set(email => email.SentOn, Timestamp.Now)
                .Inc(email => email.Attempts, 1),
            options: new FindOneAndUpdateOptions<ScheduledEmail>
            {
                ReturnDocument = ReturnDocument.After
            }
        );

        // No emails were found.  No attempts were made.
        if (toSend == null)
            return false;
        
        Log.Local(Owner.Will, $"Sending an email to {toSend.Address}!");
        
        try
        {
            AmazonSes.SendEmail(toSend.Subject, toSend.Html, toSend.Text, toSend.Address).Wait();
            return true;
        }
        catch (Exception e)
        {
            _collection.UpdateOne(
                filter: Builders<ScheduledEmail>.Filter.Eq(email => email.Id, toSend.Id),
                update: Builders<ScheduledEmail>.Update.Set(email => email.Sent, false)
            );
            if (toSend.Attempts == MAX_ATTEMPTS - 1)
                Log.Error(Owner.Will, "Failed to send a scheduled email.  It will not be retried.", data: new
                {
                    ScheduledEmail = toSend
                }, e);
            else
                Log.Warn(Owner.Will, "Failed to send a scheduled email.", data: new
                {
                    ScheduledEmail = toSend
                }, e);
            return false;
        }
    }
}

