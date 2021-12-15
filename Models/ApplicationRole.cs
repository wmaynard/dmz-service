using System;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace tower_admin_portal.Models
{
    [CollectionName("Users")]
    public class ApplicationRole : MongoIdentityRole<Guid>
    {
        
    }
}