using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Dmz.Interop;
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

public class BounceHandlerService : PlatformTimerService
{
    public static BounceHandlerService Instance { get; set; }
    private static MongoClient _mongoClient;
    private static AmazonSQSClient _sqsClient;
    
    private readonly IMongoCollection<BounceData> _bounces;
    private readonly IMongoCollection<BounceMetrics> _metrics;
    private string BounceQueueUrl { get; init; }
    
    public BounceHandlerService(DynamicConfig config) : base(intervalMS: 1_000)
    {
        Instance = this;
        
        _sqsClient ??= new AmazonSQSClient(new AwsLogin(
            accessKey: PlatformEnvironment.Require<string>("AWS_SES_ACCESS_KEY"),
            secretKey: PlatformEnvironment.Require<string>("AWS_SES_SECRET_KEY"))
        );

        BounceQueueUrl = config.Require<string>("sqsBounceUrl");
        
        string connectionString = PlatformEnvironment.Require<string>("MONGODB_GLOBAL_URI");
        MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.MaxConnectionPoolSize = PlatformMongoService<BounceData>.DEFAULT_MONGO_MAX_POOL_CONNECTION_SIZE;

        _mongoClient ??= new MongoClient(settings);
        _bounces = _mongoClient.GetDatabase("globals").GetCollection<BounceData>("bounces");
    }

    protected override void OnElapsed()
    {
        Log.Info(Owner.Will, "Testing SQS");
        string queue = "https://sqs.us-east-1.amazonaws.com/429426993201/ses-bounces";
        Task<ReceiveMessageResponse> t = _sqsClient.ReceiveMessageAsync(queue);
        t.Wait();

        foreach (BounceNotification notif in BounceNotification.FromSqsResponse(t.Result))
            ProcessNotification(notif);
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
                                RegisterBounce(recipient, isHardBounce: false);
                                break;
                            
                            default:
                                // block the user
                                RegisterBounce(recipient, isHardBounce: true);
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
        bool banned = _bounces
            .Find(Builders<BounceData>.Filter.Eq(bounce => bounce.Email, email))
            .FirstOrDefault()
            ?.Banned
            ?? false;

        if (banned)
            throw new PlatformException("Account is banned.", code: ErrorCode.None); // TODO: Error code
    }
    
    public void RegisterBounce(BounceRecipient recipient, bool isHardBounce)
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
    }
}

public class BounceMetrics : PlatformCollectionDocument
{
    public string Email { get; set; }
    public int Code { get; set; }
    public string Detail { get; set; }
    public long Timestamp { get; set; }
    public long TimeProcessed { get; set; }
}

public class BounceData : PlatformCollectionDocument
{
    private const string DB_KEY_EMAIL = "email";
    private const string DB_KEY_HARD_BOUNCES = "hard";
    private const string DB_KEY_SOFT_BOUNCES = "soft";
    private const string DB_KEY_BANNED = "ban";
    private const string DB_KEY_LAST_BOUNCE = "ts";
    
    public const string FRIENDLY_KEY_EMAIL = "email";
    public const string FRIENDLY_KEY_HARD_BOUNCES = "hard";
    public const string FRIENDLY_KEY_SOFT_BOUNCES = "soft";
    public const string FRIENDLY_KEY_BANNED = "ban";
    public const string FRIENDLY_KEY_LAST_BOUNCE = "ts";
    
    [BsonElement(DB_KEY_EMAIL)]
    [JsonPropertyName(FRIENDLY_KEY_EMAIL)]
    [SimpleIndex(unique: true)]
    public string Email { get; set; }
    
    [BsonElement(DB_KEY_BANNED)]
    [JsonPropertyName(FRIENDLY_KEY_BANNED)]
    public bool Banned { get; set; }
    
    [BsonElement(DB_KEY_HARD_BOUNCES)]
    [JsonPropertyName(FRIENDLY_KEY_HARD_BOUNCES)]
    public long LifetimeHardBounces { get; set; }
    
    [BsonElement(DB_KEY_SOFT_BOUNCES)]
    [JsonPropertyName(FRIENDLY_KEY_SOFT_BOUNCES)]
    public long LifetimeSoftBounces { get; set; }
    
    [BsonElement(DB_KEY_LAST_BOUNCE)]
    [JsonPropertyName(FRIENDLY_KEY_LAST_BOUNCE)]
    public long LastBounce { get; set; }
}

public class BounceNotification : PlatformDataModel
{
    [JsonPropertyName("Type")]
    public string Type { get; set; }
    
    [JsonPropertyName("MessageId")]
    public string MessageId { get; set; }
    
    [JsonPropertyName("TopicArn")]
    public string Topic { get; set; }
    
    [JsonPropertyName("Subject")]
    public string Subject { get; set; }
    
    [JsonPropertyName("Timestamp")]
    public DateTime Date { get; set; }
    
    [JsonPropertyName("ts"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Timestamp => Date != default
        ? (long)(Date - DateTime.UnixEpoch).TotalSeconds
        : default;
    
    [JsonInclude, JsonPropertyName("Message")] // TODO: Does this look different for complaints?
    public RumbleJson Message { private get; set; }
    
    [JsonIgnore]
    public BounceMessage BounceMessage => Message.ToModel<BounceMessage>();
    
    [JsonIgnore]
    public ComplaintMessage ComplaintMessage => throw new NotImplementedException();
    
    [JsonPropertyName("SignatureVersion")]
    public string SignatureVersion { get; set; }
    
    [JsonPropertyName("Signature")]
    public string Signature { get; set; }
    
    [JsonPropertyName("SigningCertURL")]
    public string CertificateUrl { get; set; }
    
    [JsonPropertyName("UnsubscribeURL")]
    public string UnsubscribeUrl { get; set; }

    [JsonIgnore]
    public string ReceiptHandle { get; private set; }
    
    private BounceNotification SetReceiptHandle(string receipt)
    {
        ReceiptHandle = receipt;
        return this;
    }

    public static BounceNotification[] FromSqsResponse(ReceiveMessageResponse response) => response
        .Messages
        .Select(message => ((RumbleJson)message.Body).ToModel<BounceNotification>()?.SetReceiptHandle(message.ReceiptHandle))
        .ToArray();
}

public class ComplaintMessage : PlatformDataModel
{
    
}

public class BounceMessage : PlatformDataModel
{
    public const string JSON_KEY_IN_PARENT = "Message";
    
    [JsonPropertyName("eventType")]
    public string EventType { get; set; }
    
    [JsonPropertyName("bounce")]
    public Bounce Bounce { private get; set; }

    [JsonPropertyName("mail")]
    public RumbleJson MailDetails { get; set; }
    
    [JsonPropertyName("feedbackId")]
    public string FeedbackId { get; set; }
    
    #region ChildAccessors
    [JsonPropertyName("bounceType")]
    public string Type => Bounce?.Type;
        
    [JsonPropertyName("bounceSubType")]
    public string SubType => Bounce?.SubType;

    [JsonPropertyName("bouncedRecipients")]
    public BounceRecipient[] Recipients => Bounce?.Recipients;
    
    [JsonPropertyName("failedAddresses")]
    public string[] Addresses => Bounce?.Addresses;

    [JsonPropertyName("ts"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Timestamp => Bounce?.Timestamp ?? default;
    
    [JsonPropertyName("reportingMTA")]
    public string Reporter => Bounce?.Reporter;
    #endregion ChildAccessors
}

public class Bounce : PlatformDataModel
{
    [JsonPropertyName("feedbackId")]
    public string FeedbackId { get; set; }
        
    [JsonPropertyName("bounceType")]
    public string Type { get; set; }
        
    [JsonPropertyName("bounceSubType")]
    public string SubType { get; set; }
        
    [JsonPropertyName("bouncedRecipients")]
    public BounceRecipient[] Recipients { get; set; }
    
    [JsonPropertyName("failedAddresses")]
    public string[] Addresses => Recipients.Select(recipient => recipient.Address).ToArray();
    
    [JsonPropertyName("timestamp")]
    public DateTime Date { get; set; }

    [JsonPropertyName("ts"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Timestamp => Date != default
        ? (long)(Date - DateTime.UnixEpoch).TotalSeconds
        : default;
    
    [JsonPropertyName("reportingMTA")]
    public string Reporter { get; set; }
}

public class BounceRecipient : PlatformDataModel
{
    [JsonPropertyName("emailAddress")]
    public string Address { get; set; }
    
    [JsonPropertyName("action")]
    public string Action { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("code")]
    public int Code => int.TryParse(Status.Replace(".", ""), out int output)
        ? output
        : -1;
    
    [JsonPropertyName("diagnosticCode")]
    public string Detail { get; set; }
}


























