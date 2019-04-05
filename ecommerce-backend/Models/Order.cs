using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
            Location = new Location();
        }

        public int OrderId { get; set; }
        public int? CustomerId { get; set; }
        public int LocationId { get; set; }
        public DateTime OrderDate { get; set; }

        [Required]
        public decimal Total { get; set; }

        [Required]
        public decimal SubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public string PstNumber { get; set; }
        public string Notes { get; set; }
        public string PoNumber { get; set; }
        public string Status { get; set; }
        public bool IsAccountReturn { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByUserId { get; set; }
        public int? OriginalOrderId { get; set; }

        [NotMapped]
        public string AuthCode { get; set; }

        [NotMapped]
        public string CreatedByUserName { get; set; }

        public string Email { get; set; }

        public ICollection<OrderDetail> OrderDetail { get; set; }
        public ICollection<OrderTax> OrderTax { get; set; }
        public ICollection<OrderPayment> OrderPayment { get; set; }

        public Customer Customer { get; set; }
        public Location Location { get; set; }
        public decimal RestockingFeePercent { get; set; }
        public decimal RestockingFeeAmount { get; set; }
    }

    public enum OrderStatus {
        Paid,
        OnHold,
        Draft,
        Account,
        Return
    }
}
