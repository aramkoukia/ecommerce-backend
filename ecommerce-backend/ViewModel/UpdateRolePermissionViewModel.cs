using System.Collections.Generic;

namespace EcommerceApi.Controllers
{
    public class UpdateRolePermissionViewModel
    {
        public string RoleName { get; set; }
        public List<string> Claims { get; set; }
    }
}