using System.Collections.Generic;

namespace EcommerceApi.Controllers
{
    public class UpdateUserPermissionViewModel
    {
        public string Email { get; set; }
        public List<string> RoleIds { get; set; }
        public List<int> LocationIds { get; set; }
    }
}