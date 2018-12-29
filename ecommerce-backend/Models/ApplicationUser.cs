using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string GivenName { get; set; }

        [NotMapped]
        public IList<string> Roles { get; set; }
    }
}
