using Rumble.Platform.Common.Web;
using tower_admin_portal.Models;

namespace tower_admin_portal.Services
{
    public class AccountService : PlatformMongoService<Account>
    {
        public AccountService() : base(collection: "accounts")
        {
            
        }
    }
}