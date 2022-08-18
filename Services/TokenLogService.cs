using System.Collections.Generic;
using MongoDB.Driver;
using Rumble.Platform.Common.Services;
using TowerPortal.Models.Token;

namespace TowerPortal.Services;

public class TokenLogService : PlatformMongoService<TokenLog>
{
    public TokenLogService() : base(collection: "tokenLogs")
    { }

    public List<TokenLog> GetLogs()
    {
        return _collection.Find(filter: log => true).SortByDescending(log => log.Timestamp).ToList();
    }
}