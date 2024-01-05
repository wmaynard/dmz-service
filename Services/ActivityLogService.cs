using Dmz.Models.Portal;
using RCL.Logging;
using Rumble.Platform.Common.Minq;
using Rumble.Platform.Common.Utilities;

namespace Dmz.Services;

public class ActivityLogService : MinqTimerService<AuditLog>
{
    public ActivityLogService() : base("activities", IntervalMs.FourHours) { }

    public AuditLog[] Page(int size, int page, string accountId, out long remaining) => mongo
        .Where(query =>
        {
            if (!string.IsNullOrWhiteSpace(accountId))
                query.EqualTo(log => log.Who.AccountId, accountId);
        })
        .Sort(sort => sort.OrderByDescending(log => log.CreatedOn))
        .Page(size, page, out remaining);

    protected override void OnElapsed() => mongo
        .Where(query => query.LessThanOrEqualTo(log => log.CreatedOn, Timestamp.SixMonthsAgo))
        .OnRecordsAffected(result => Log.Info(Owner.Will, "Old DMZ activity logs deleted.", data: new
        {
            Affected = result.Affected
        }))
        .Delete();
}