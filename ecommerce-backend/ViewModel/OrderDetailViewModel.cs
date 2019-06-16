using EcommerceApi.Models;
using System.Collections.Generic;

namespace EcommerceApi.ViewModel
{
    public class OrderDetailViewModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal Total { get; set; }
        public string Package { get; set; }
        public decimal? AmountInMainPackage { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
    }
}
