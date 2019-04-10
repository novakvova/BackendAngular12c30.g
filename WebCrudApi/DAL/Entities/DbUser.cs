using Microsoft.AspNetCore.Identity;

namespace WebCrudApi.DAL.Entities
{
    public class DbUser : IdentityUser
    {

        public virtual UserProfile UserProfile { get; set; }
    }
}