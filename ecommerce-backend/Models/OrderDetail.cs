using System;

namespace EcommerceApi.Models
{
    [Serializable]
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
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
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
