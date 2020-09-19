namespace EcommerceApi.Controllers
{
    public class UpdateUserViewModel
    {
        public string Email { get; set; }
        public string GivenName { get; set; }
        public string UserName { get; set; }
        public bool Disabled { get; set; }
    }
}