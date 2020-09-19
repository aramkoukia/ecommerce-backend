using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string GivenName { get; set; }
        public string AuthCode { get; set; }
        public bool Disabled { get; set; }

        [NotMapped]
        public string Roles { get; set; }

        [NotMapped]
        public string Locations { get; set; }
    }
}
