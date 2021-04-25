using System;

namespace EcommerceApi.Models
{
    [Serializable]
    public class CustomerUsers
    {
        public int CustomerId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreationMethod { get; set; }
        public string CreatorUserId { get; set; }
    }
}