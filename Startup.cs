using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dmz.Filters;
using Dmz.Interop;
using Dmz.Models.Permissions;
using Dmz.Models.Portal;
using Dmz.Services;
using Dmz.Utilities;
using DnsClient;
using DnsClient.Protocol;
using Microsoft.Extensions.Hosting;
using RCL.Logging;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Data;

namespace Dmz;

public class Startup : PlatformStartup
{
    protected override PlatformOptions ConfigureOptions(PlatformOptions options) => options
        .SetRegistrationName("DMZ")
        .SetTokenAudience(Audience.DmzService)
        .SetProjectOwner(Owner.Will)
        .SetPerformanceThresholds(warnMS: 30_000, errorMS: 60_000, criticalMS: 90_000)
        .DisableFeatures(CommonFeature.ConsoleObjectPrinting)
        .DisableFilters(CommonFilter.Performance)
        .AddFilter<PermissionsFilter>()
        .AddFilter<AuditFilter>()
        .AddFilter<ForwardingExceptionFilter>()
        .OnReady(_ =>
        {
            PlatformService.Optional<RoleService>()?.EnsureSuperUserExists();
            // PlatformService.Optional<AccountService>()?.Insert(new Account
            // {
            //     Name = "Postman",
            //     Email = "test@rumbleentertainment.com",
            //     Permissions = new Passport(Passport.PassportType.Nonprivileged)
            // });
        });
}

