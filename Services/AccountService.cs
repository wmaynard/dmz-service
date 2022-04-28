using Rumble.Platform.Common.Web;
using TowerPortal.Models;

namespace TowerPortal.Services;
public class AccountService : PlatformMongoService<Account>
{
    public AccountService() : base(collection: "accounts")
    {
        
    }
}