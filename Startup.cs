using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dmz.Filters;
using Dmz.Interop;
using Dmz.Utilities;
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
            try
            {
                #if DEBUG
                // await AmazonSes.Nuke();
                #endif
                await AmazonSes.CreateOrUpdateTemplate(
                    name: PlayerServiceEmail.TEMPLATE_CONFIRMATION,
                    subject: "Confirm Your Rumble Account",
                    html: @"
<html>
<body>
    <h1>Confirm your email address</h1>
    <br />
    Please confirm your email address by tapping on the link below.
    <br />
    <a href='{endpoint}?id={accountId}&code={code}'>Confirm your account!</a>
    <br />
    This link will expire in {duration}.
</body>
</html>",
                    backupText: @"
Confirm your email address

Copy the link below and paste it into your browser to confirm your account.

{endpoint}?id={accountId}&code={code}

This link will expire in {duration}."
                );
                await AmazonSes.CreateOrUpdateTemplate(
                    name: PlayerServiceEmail.TEMPLATE_2FA,
                    subject: "Your Rumble Verification Code",
                    html: @"
<html>
<body>
    <h1>Here is your verification code for your Rumble account:</h1>
    <br />
    <h1>{code}</h1>
    <br />
    This code expires in {duration}.
    <br />
    <h4>Not you?  It's safe to ignore this email.</h4>
</body>
<html>",
                    backupText: @"
Here is your verification code for your Rumble account:

{code}

This code expires in {duration}.

Not you?  It's safe to ignore this email."
                );
                await AmazonSes.CreateOrUpdateTemplate(
                    name: PlayerServiceEmail.TEMPLATE_NEW_DEVICE_NOTIFICATION,
                    subject: "New Login From {device}",
                    html: @"
<html>
<body>
    <h1>A new device was logged into your account on {timestamp}.<h1>
    <br />
    <h4>Not you?  Contact support so we can assist you.</h4>
</body>
<html>",
                    backupText: @"
A new device was logged into your account on {timestamp}.

Not you?  Contact support so we can assist you."
                );
                await AmazonSes.CreateOrUpdateTemplate(
                    name: PlayerServiceEmail.TEMPLATE_PASSWORD_RESET,
                    subject: "Rumble Password Reset",
                    html: @"
<html>
<body>
    <h1>Here is your verification code for your Rumble account:</h1>
    <br />
    <h1>{code}</h1>
    <br />
    This code expires in {duration}.
    <br />
    Please enter this code in-game to reset your password and sign in.
    <br />
    <h4>Not you?  It's safe to ignore this email.</h4>
</body>
<html>",
                    backupText: @"
Here is your verification code for your Rumble account:

{code}

This code expires in {duration}.

Please enter this code in-game to reset your password and sign in.

Not you?  It's safe to ignore this email"
                );
            }
            catch (Exception e)
            {
                Log.Local(Owner.Will, "Unable to set up SES templates.", exception: e);
            }
        });
}