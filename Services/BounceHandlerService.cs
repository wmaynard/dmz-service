using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Dmz.Exceptions;
using Dmz.Interop;
using Dmz.Models.Bounces;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Services;

public class BounceHandlerService : PlatformMongoTimerService<BounceData>
{
    public const string KEY_DYNAMIC_CONFIG_QUEUE_URL = "sqsBounceUrl";
    public static BounceHandlerService Instance { get; set; }
    private static MongoClient _mongoClient;
    private static AmazonSQSClient _sqsClient;
    
    private readonly IMongoCollection<BounceData> _bounces;
    private readonly IMongoCollection<BounceDataPoint> _dataPoints;
    private string BounceQueueUrl { get; init; }
    
    public BounceHandlerService(DynamicConfig config) : base(collection: "temp", intervalMs: 1_000)
    {
        Instance = this;
        
        _sqsClient ??= new AmazonSQSClient(new AwsLogin(
            accessKey: PlatformEnvironment.Require<string>("AWS_SES_ACCESS_KEY"),
            secretKey: PlatformEnvironment.Require<string>("AWS_SES_SECRET_KEY"))
        );

        BounceQueueUrl = config.Require<string>(KEY_DYNAMIC_CONFIG_QUEUE_URL);

        if (string.IsNullOrWhiteSpace(BounceQueueUrl))
            throw new PlatformException($"Unable to start {GetType().FullName}; missing DC var {KEY_DYNAMIC_CONFIG_QUEUE_URL}");
        
        string connectionString = PlatformEnvironment.Require<string>("MONGODB_GLOBAL_URI");
        // MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);
        // settings.MaxConnectionPoolSize = PlatformMongoService<BounceData>.DEFAULT_MONGO_MAX_POOL_CONNECTION_SIZE;

        _mongoClient ??= CreateClient(connectionString);
        _bounces = _mongoClient.GetDatabase("globals").GetCollection<BounceData>("bounces");
        _dataPoints = _mongoClient.GetDatabase("globals").GetCollection<BounceDataPoint>("bounceData");

        Console.WriteLine(EmailRegex.IsValid("foo@g.mail.com"));
        EmailRegex.IsValid("sflancer06@47.com");
    }

    protected override void OnElapsed()
    {
        BounceNotification[] notifs = Array.Empty<BounceNotification>();
        do
        {
            Task<ReceiveMessageResponse> task = _sqsClient.ReceiveMessageAsync(BounceQueueUrl);
            task.Wait();
            notifs = BounceNotification.FromSqsResponse(task.Result);
            
            foreach (BounceNotification notif in notifs)
                ProcessNotification(notif);
        } while (notifs.Any());
    }

    private void ProcessNotification(BounceNotification notif)
    {
        try
        {
            switch (notif.BounceMessage.EventType)
            {
                case "Bounce":
                    foreach (BounceRecipient recipient in notif.BounceMessage.Recipients)
                        switch (recipient.Code)
                        {
                            case 0:
                                Log.Warn(Owner.Will, "Unknown bounce code", data: new
                                {
                                    sqsNotif = notif
                                });
                                break;
                            case < 500: // soft bounces
                            case 522: // Mailbox is full
                            case 552: // Mailbox limit exceeded
                            case > 560: // Others that seem rare
                                RegisterBounce(recipient, notif.Timestamp, isHardBounce: false);
                                break;
                            
                            default:
                                // block the user
                                RegisterBounce(recipient, notif.Timestamp, isHardBounce: true);
                                break;
                        }
                    break;
                case "Complaint": //??
                    break;
                default:
                    Log.Warn(Owner.Will, "Unhandled SQS notification type", data: new
                    {
                        sqsNotif = notif
                    });
                    break;
            }

            _sqsClient.DeleteMessageAsync(BounceQueueUrl, notif.ReceiptHandle).Wait();
        }
        catch (Exception e)
        {
            Log.Error(Owner.Will, "Unable to process SQS bounce notification.", data: new
            {
                BounceNotification = notif
            }, exception: e);
        }
    }
    
    public void EnsureNotBanned(string email)
    {
        bool banned = false;
        bool emailInvalid = !EmailRegex.IsValid(email);
        
        if (emailInvalid)
            RegisterValidationBan(email);
        else
            banned = _bounces
            .Find(Builders<BounceData>.Filter.Eq(bounce => bounce.Email, email))
            .FirstOrDefault()
            ?.Banned
            ?? false;

        if (emailInvalid || banned)
            throw new EmailBannedException(email);
    }

    public void RegisterValidationBan(string email)
    {
        Log.Local(Owner.Will, $"Registering validation hard bounce for {email}");
        BounceData data = _bounces
            .FindOneAndUpdate(
                filter: Builders<BounceData>.Filter.Eq(data => data.Email, email),
                update: Builders<BounceData>.Update
                        .Inc(data => data.LifetimeHardBounces, 1)
                        .Set(data => data.Banned, true)
                .Set(data => data.LastBounce, Timestamp.UnixTime),
                options: new FindOneAndUpdateOptions<BounceData>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                }
            );
        _dataPoints.InsertOne(new BounceDataPoint
        {
            Code = 999,
            Detail = "Email address failed Rumble's regex and/or System.Net.MailAddress validation.",
            Email = email,
            TimeProcessed = Timestamp.UnixTime,
            Timestamp = Timestamp.UnixTime
        });
    }
    
    public void RegisterBounce(BounceRecipient recipient, long timestamp, bool isHardBounce)
    {
        Log.Local(Owner.Will, $"Registering {(isHardBounce ? "hard" : "soft")} bounce for {recipient.Address}");
        BounceData data = _bounces
            .FindOneAndUpdate(
                filter: Builders<BounceData>.Filter.Eq(data => data.Email, recipient.Address),
                update: (isHardBounce
                    ? Builders<BounceData>.Update
                        .Inc(data => data.LifetimeHardBounces, 1)
                        .Set(data => data.Banned, true)
                    : Builders<BounceData>.Update.Inc(data => data.LifetimeSoftBounces, 1))
                .Set(data => data.LastBounce, Timestamp.UnixTime),
                options: new FindOneAndUpdateOptions<BounceData>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                }
            );
        if (!isHardBounce && data.LifetimeSoftBounces % 50 == 0)
            Log.Warn(Owner.Will, "An email is seeing a lot of soft bounces", data: new
            {
                BounceData = data
            });
        _dataPoints.InsertOne(new BounceDataPoint
        {
            Code = recipient.Code,
            Detail = recipient.Detail,
            Email = recipient.Address,
            TimeProcessed = Timestamp.UnixTime,
            Timestamp = timestamp
        });
    }
}