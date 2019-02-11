namespace EcommerceApi.Controllers
{
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string NewPassword { get; set; }
    }
}