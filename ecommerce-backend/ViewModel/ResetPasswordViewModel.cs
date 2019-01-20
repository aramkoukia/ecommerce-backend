using System.Collections.Generic;

namespace EcommerceApi.Controllers
{
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}