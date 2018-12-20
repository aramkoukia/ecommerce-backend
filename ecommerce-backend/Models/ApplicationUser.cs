using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EcommerceApi.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string GivenName { get; set; }
    }
}
