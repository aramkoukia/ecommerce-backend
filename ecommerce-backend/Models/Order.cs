using System;
using System.Collections.Generic;

namespace EcommerceApi.Models
{
    public class Order
    {
        public Order()
        {
            OrderDetail = new HashSet<OrderDetail>();
            OrderTax = new HashSet<OrderTax>();
            OrderPayment = new HashSet<OrderPayment>();
            Customer = new Customer();
        }

        public int? OrderId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? DiscountId { get; set; }
        public string PstNumber { get; set; }
        public string Notes { get; set; }
        public string PoNumber { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedByUserId { get; set; }

        public ICollection<OrderDetail> OrderDetail { get; set; }
        public ICollection<OrderTax> OrderTax { get; set; }
        public ICollection<OrderPayment> OrderPayment { get; set; }

        public Customer Customer { get; set; }
    }
}
