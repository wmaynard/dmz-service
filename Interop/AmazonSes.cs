global using SesClient = Amazon.SimpleEmailV2.AmazonSimpleEmailServiceV2Client;
global using AwsLogin = Amazon.Runtime.BasicAWSCredentials;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Dmz.Models;
using Dmz.Services;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Interop;

public static class AmazonSes
{
    private const string FROM_EMAIL = "Rumble Entertainment <noreply@rumbleentertainment.com>";
    private const string CHARSET = "UTF-8";
    
    private static AmazonSimpleEmailServiceV2Client _client;
    
    public static AmazonSimpleEmailServiceV2Client Client => GetOrInitializeClient();

    private static AmazonSimpleEmailServiceV2Client GetOrInitializeClient() =>_client ??= new AmazonSimpleEmailServiceV2Client(new BasicAWSCredentials(
        accessKey: PlatformEnvironment.Require<string>("AWS_SES_ACCESS_KEY"), 
        secretKey: PlatformEnvironment.Require<string>("AWS_SES_SECRET_KEY"))
    );

    public static async Task CreateOrUpdateTemplate(string name, string subject, string html, string backupText)
    {
        Cleanse(ref name);
        EmailTemplateContent content = new EmailTemplateContent
        {
            Html = html,
            Subject = subject,
            Text = backupText
        };

        if (TemplateNames.Contains(name))
            await Client.UpdateEmailTemplateAsync(request: new UpdateEmailTemplateRequest
            {
                TemplateName = name,
                TemplateContent = content
            });
        else
            await Client.CreateEmailTemplateAsync(request: new CreateEmailTemplateRequest
            {
                TemplateName = name,
                TemplateContent = content
            });
    }

    public static async Task SendEmail(string email, string templateName, RumbleJson replacements)
    {
        Cleanse(ref templateName);

        EmailTemplateContent content = await GetTemplate(templateName);
        string charset = "UTF-8";

        string html = Replace(content.Html, replacements);
        string text = Replace(content.Text, replacements);
        string subject = Replace(content.Subject, replacements);

        await SendEmail(subject, html, text, email);
    }

    /// <summary>
    /// Creates an email that's scheduled to be sent out at a later time.  The template is loaded and crafted immediately
    /// during this call. 
    /// </summary>
    /// <param name="address">The email address to send this email to.</param>
    /// <param name="delay">The duration, in seconds, to wait before the email will send.</param>
    /// <param name="templateName">The template to use for the email.</param>
    /// <param name="replacements">The replacements to make in the template.</param>
    /// <returns></returns>
    public static ScheduledEmail CraftScheduledEmail(string address, long delay, string templateName, RumbleJson replacements = null)
    {
        Task<EmailTemplateContent> task = GetTemplate(templateName);
        task.Wait();
        EmailTemplateContent content = task.Result;
        // EmailTemplateContent content = await GetTemplate(templateName);

        return new ScheduledEmail
        {
            Address = address,
            SendAfter = Timestamp.UnixTime + delay,
            Subject = Replace(content.Subject, replacements),
            Html = Replace(content.Html, replacements),
            Text = Replace(content.Text, replacements)
        };
    }

    public static async Task<SendEmailResponse> SendEmail(string subject, string html, string text, params string[] emails)
    {
        SendEmailResponse response = await Client.SendEmailAsync(new SendEmailRequest
        {
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = CHARSET,
                            Data = html
                        },
                        Text = new Content
                        {
                            Charset = CHARSET,
                            Data = text
                        }
                    },
                    Subject = new Content
                    {
                        Charset = CHARSET,
                        Data = subject
                    }
                }
            },
            Destination = new Destination
            {
                ToAddresses = emails.ToList()
            },
            ConfigurationSetName = null,
            EmailTags = null,
            FeedbackForwardingEmailAddress = null,
            FeedbackForwardingEmailAddressIdentityArn = null,
            FromEmailAddress = FROM_EMAIL,
            FromEmailAddressIdentityArn = null,
            ListManagementOptions = null,
            ReplyToAddresses = null
        });

        return ((int)response.HttpStatusCode).Between(200, 299)
            ? response
            : throw new PlatformException("Unable to send email.");
    }

    public static string[] TemplateNames
    {
        get
        {
            Task<ListEmailTemplatesResponse> task = Client.ListEmailTemplatesAsync(new ListEmailTemplatesRequest());
            task.Wait();
            ListEmailTemplatesResponse response = task.Result;

            return response
                .TemplatesMetadata
                .Select(template => template.TemplateName)
                .ToArray();
        }
    }

    private static string Replace([NotNull] string target, RumbleJson replacements)
    {
        // TODO: These replacements are a kluge for the sed command issues in the CI script
        target = target
            .Replace("__name__", "&")
            .Replace("__subject", "&")
            .Replace("__html__", "&")
            .Replace("__text__", "&");
        if (replacements == null || !replacements.Any())
            return target;

        foreach (string replacement in replacements.Keys)
        {
            int depth = 5;
            do
            {
                target = target.Replace($"{{{replacement}}}", replacements.Require<string>(replacement));
            } while (target.Contains($"{{{replacement}}}") && --depth > 0);
        }

        return target;
    }

    private static async Task<EmailTemplateContent> GetTemplate(string name)
    {
        Cleanse(ref name);

        GetEmailTemplateResponse response = await Client.GetEmailTemplateAsync(new GetEmailTemplateRequest
        {
            TemplateName = name
        });

        return response.TemplateContent;
    }

    private static void Cleanse(ref string templateName)
    {
        do
        {
            templateName = templateName.Replace("  ", " ");
        } while (templateName.Contains("  "));

        templateName = templateName.ToLower().Replace(" ", "-");

        if (!templateName.StartsWith(PlatformEnvironment.Deployment))
            templateName = $"{PlatformEnvironment.Deployment}-{templateName}";
    }
    
#if DEBUG
    public static async Task Nuke()
    {
        foreach (string template in TemplateNames)
        {
            await Client.DeleteEmailTemplateAsync(new DeleteEmailTemplateRequest
            {
                TemplateName = template
            });
            Thread.Sleep(1500);
        }
    }
#endif
}