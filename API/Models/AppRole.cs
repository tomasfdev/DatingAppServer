using Microsoft.AspNetCore.Identity;

namespace API.Models
{
    public class AppRole : IdentityRole<int>
    {
        public virtual ICollection<AppUserRole> UserRoles { get; set; }
    }
}
