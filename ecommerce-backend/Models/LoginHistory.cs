using System;

namespace EcommerceApi.Models
{
    public class LoginHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string ClientIp { get; set; }
        public string LoginType { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
