using EcommerceApi.Models;
using System.Collections.Generic;

namespace EcommerceApi.ViewModel
{
    public class CustomerAwaitingPaymentDetail
    {
        public int OrderId { get; set; }
        public int LocationId { get; set; }
        public string OrderDate { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public int DiscountId { get; set; }
        public string PstNumber { get; set; }
        public string Notes { get; set; }
        public string PoNumber { get; set; }
        public string Status { get; set; }
        public string GivenName { get; set; }
        public decimal PaidAmount { get; set; }
        public string LocationName { get; set; }
        public string PaymentTypeName { get; set; }
        public string CompanyName { get; set; }
        public string DueDate { get; set; }
        public string OverDue { get; set; }
        public string PstCharged { get; set; }
        public decimal PstAmount { get; set; }
        public int CustomerId { get; set; }
    }
}
