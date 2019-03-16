using System;

namespace EcommerceApi.Models
{
    public class CustomerStoreCredit
    {
        public CustomerStoreCredit()
        {
            Customer = new Customer();
        }
        public int CustomerStoreCreditId { get; set; }
        public int CustomerId { get; set; }
        public string Amount { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByUserId { get; set; }

        public Customer Customer { get; set; }
    }
}
