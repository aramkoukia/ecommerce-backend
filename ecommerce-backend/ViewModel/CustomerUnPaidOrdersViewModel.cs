using System;

namespace EcommerceApi.ViewModel
{
    public class CustomerUnPaidOrdersViewModel
    {
        public int OrderId { get; set; }
        public string PoNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
        public string CompanyName { get; set; }
        public string CustomerCode { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
    }
}