using RCL.Logging;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Web;
using TowerPortal.Filters;

namespace TowerPortal;

public class Startup : PlatformStartup
{
    protected override PlatformOptions Configure(PlatformOptions options) => options
        .SetRegistrationName("Portal")
        .SetProjectOwner(Owner.Nathan)
        .SetPerformanceThresholds(warnMS: 30_000, errorMS: 60_000, criticalMS: 90_000)
        // .DisableFeatures(CommonFeature.ConsoleObjectPrinting)
        .DisableFilters(CommonFilter.Performance)
        .AddFilter<PermissionsFilter>();
}