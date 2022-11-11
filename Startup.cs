using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dmz.Filters;
using Dmz.Interop;
using RCL.Logging;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Data;

namespace Dmz;

public class Startup : PlatformStartup
{
    protected override PlatformOptions ConfigureOptions(PlatformOptions options) => options
        .SetRegistrationName("DMZ")
        .SetTokenAudience(Audience.DmzService)
        .SetProjectOwner(Owner.Nathan)
        .SetPerformanceThresholds(warnMS: 30_000, errorMS: 60_000, criticalMS: 90_000)
        .DisableFeatures(CommonFeature.ConsoleObjectPrinting)
        .DisableFilters(CommonFilter.Performance)
        .AddFilter<PermissionsFilter>()
        .OnBeforeStartup(async () =>
        {
            // This is just temporary.  Once we have template management in Portal, this can go the way of the dodo.
            try
            {
                #if DEBUG
                await AmazonSes.Nuke();
                #endif
                
                await AmazonSes.CreateOrUpdateTemplate(
                    name: "Hello World",
                    subject: "Hello World",
                    html: "<html><body><h1>Hello, World!</h1>This is a test email from dmz-service.</body></html>",
                    backupText: "Hello, World!"
                );
                await AmazonSes.CreateOrUpdateTemplate(
                    name: PlayerServiceEmail.TEMPLATE_CONFIRMATION,
                    subject: "Confirm Your Rumble Account",
                    html: "<html><body><h1>Welcome to Towers & Titans!</h1><a href=\"{endpoint}?id={accountId}&code={code}\">Confirm your account!</a></body><html>",
                    backupText: "Welcome to Towers & Titans!\n\nConfirm your account by copying and pasting the following link into your browser: {endpoint}?id={accountId}&code={code}"
                );
                await AmazonSes.CreateOrUpdateTemplate(
                    name: PlayerServiceEmail.TEMPLATE_2FA,
                    subject: "Another Device Logged In",
                    html: "<html><body><h1>New Login From {device}.</h1><br /><br /><h3>Enter the following code in the game to grant access: <b>{code}</b></h3><b4 />This code expires in {duration}.<h4>Not you?  It's safe to ignore this email.</h4><br /></body><html>",
                    backupText: ""
                );
            }
            catch (Exception e)
            {
                Log.Local(Owner.Will, "Unable to set up SES templates.", exception: e);
            }
        });
}