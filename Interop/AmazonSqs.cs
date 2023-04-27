using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SQS.Model;
using Dmz.Services;
using Rumble.Platform.Data;

namespace Dmz.Interop;

public class AmazonSqs
{
    /// <summary>Process bounces received from Amazon SES via Amazon SQS.</summary>
    /// <param name="response">The response from the Amazon SQS bounces queue 
    /// to a ReceiveMessage request. This object contains the Amazon SES  
    /// bounce notification.</param> 
    public static void ProcessQueuedBounce(ReceiveMessageResponse response)
    {
        if (!response.Messages.Any())
            return;
        
        foreach (Message m in response.Messages)
        {
            // First, convert the Amazon SNS message into a JSON object.
            RumbleJson body = m.Body;
            RumbleJson message = body.Require<RumbleJson>("Message");

            try
            {
                BounceMessage bm = body.Require<BounceMessage>(BounceMessage.JSON_KEY_IN_PARENT);
                BounceNotification notif = ((RumbleJson)m.Body).ToModel<BounceNotification>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            // var notification = Newtonsoft.Json.JsonConvert.DeserializeObject<AmazonSqsNotification>(m.Body);

            // Now access the Amazon SES bounce notification.
            // var bounce = Newtonsoft.Json.JsonConvert.DeserializeObject<AmazonSesBounceNotification>(notification.Message);

            // switch (bounce.Bounce.BounceType)
            // {
            //     case "Transient":
            //         // Per our sample organizational policy, we will remove all recipients 
            //         // that generate an AttachmentRejected bounce from our mailing list.
            //         // Other bounces will be reviewed manually.
            //         switch (bounce.Bounce.BounceSubType)
            //         {
            //             case "AttachmentRejected":
            //                 foreach (var recipient in bounce.Bounce.BouncedRecipients)
            //                     RemoveFromMailingList(recipient.EmailAddress);
            //                 break;
            //             default:
            //                 ManuallyReviewBounce(bounce);
            //                 break;
            //         }
            //         break;
            //     default:
            //         // Remove all recipients that generated a permanent bounce or an unknown bounce.
            //         foreach (var recipient in bounce.Bounce.BouncedRecipients)
            //             RemoveFromMailingList(recipient.EmailAddress);
            //         break;
            // }
        }
    }
    
    /// <summary>Process complaints received from Amazon SES via Amazon SQS.</summary>
    /// <param name="response">The response from the Amazon SQS complaint queue 
    /// to a ReceiveMessage request. This object contains the Amazon SES 
    /// complaint notification.</param>
    public static void ProcessQueuedComplaint(ReceiveMessageResponse response)
    {
        if (!response.Messages.Any())
            return;
        
        foreach (Message message in response.Messages)
        {
            RumbleJson notification = message.Body;

            // Now access the Amazon SES complaint notification.
            // var complaint = Newtonsoft.Json.JsonConvert.DeserializeObject<AmazonSesComplaintNotification>(notification.Message);

            // foreach (var recipient in complaint.Complaint.ComplainedRecipients)
            // {
            //     // Remove the email address that complained from our mailing list.
            //     RemoveFromMailingList(recipient.EmailAddress);
            // }
        }
    }
}

/// <summary>Represents the bounce or complaint notification stored in Amazon SQS.</summary>
class AmazonSqsNotification
{
    public string Type { get; set; }
    public string Message { get; set; }
}

/// <summary>Represents an Amazon SES bounce notification.</summary>
class AmazonSesBounceNotification
{
    public string NotificationType { get; set; }
    public AmazonSesBounce Bounce { get; set; }
}
/// <summary>Represents meta data for the bounce notification from Amazon SES.</summary>
class AmazonSesBounce
{
    public string BounceType { get; set; }
    public string BounceSubType { get; set; }
    public DateTime Timestamp { get; set; }
    public List<AmazonSesBouncedRecipient> BouncedRecipients { get; set; }
}
/// <summary>Represents the email address of recipients that bounced
/// when sending from Amazon SES.</summary>
class AmazonSesBouncedRecipient
{
    public string EmailAddress { get; set; }
}

/// <summary>Represents an Amazon SES complaint notification.</summary>
class AmazonSesComplaintNotification
{
    public string NotificationType { get; set; }
    public AmazonSesComplaint Complaint { get; set; }
}
/// <summary>Represents the email address of individual recipients that complained 
/// to Amazon SES.</summary>
class AmazonSesComplainedRecipient
{
    public string EmailAddress { get; set; }
}
/// <summary>Represents meta data for the complaint notification from Amazon SES.</summary>
class AmazonSesComplaint
{
    public List<AmazonSesComplainedRecipient> ComplainedRecipients { get; set; }
    public DateTime Timestamp { get; set; }
    public string MessageId { get; set; }
}