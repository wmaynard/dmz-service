global using SesClient = Amazon.SimpleEmailV2.AmazonSimpleEmailServiceV2Client;
global using AwsLogin = Amazon.Runtime.BasicAWSCredentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Interop;

public static class AmazonSes
{
    private const string FROM_EMAIL = "noreply@rumbleentertainment.com";
    
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

        EmailContent _email = new EmailContent
        {
            Simple = new Message
            {
                Body = new Body
                {
                    Html = new Content
                    {
                        Charset = charset,
                        Data = html
                    },
                    Text = new Content
                    {
                        Charset = charset,
                        Data = text
                    }
                },
                Subject = new Content
                {
                    Charset = charset,
                    Data = subject
                }
            }
        };

        SendEmailResponse response = await Client.SendEmailAsync(new SendEmailRequest
        {
            Content = _email,
            Destination = new Destination
            {
                ToAddresses = new List<string> { email }
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
        if (!((int)response.HttpStatusCode).Between(200, 299))
            throw new PlatformException("Unable to send email.");
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

    private static string Replace(string target, RumbleJson replacements)
    {
        if (replacements == null || !replacements.Any())
            return null;

        foreach (string replacement in replacements.Keys)
        {
            do
            {
                target = target.Replace($"{{{replacement}}}", replacements.Require<string>(replacement));
            } while (target.Contains($"{{{replacement}}}"));
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
    }
    
#if DEBUG
    public static async Task Nuke()
    {
        foreach (string template in TemplateNames)
            await Client.DeleteEmailTemplateAsync(new DeleteEmailTemplateRequest
            {
                TemplateName = template
            });
    }
#endif
}