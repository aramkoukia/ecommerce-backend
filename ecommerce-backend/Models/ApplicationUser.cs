using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string GivenName { get; set; }

        [NotMapped]
        public string Roles { get; set; }

        [NotMapped]
        public string Locations { get; set; }
    }
}
