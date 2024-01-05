using Dmz.Models.Portal;
using Rumble.Platform.Common.Minq;

namespace Dmz.Services;

public class ActivityLogService : MinqService<AuditLog>
{
    public ActivityLogService() : base("activities") { }

    public AuditLog[] Page(int size, int page, string accountId, out long remaining) => mongo
        .Where(query =>
        {
            if (!string.IsNullOrWhiteSpace(accountId))
                query.EqualTo(log => log.Who.AccountId, accountId);
        })
        .Sort(sort => sort.OrderByDescending(log => log.CreatedOn))
        .Page(size, page, out remaining);
}