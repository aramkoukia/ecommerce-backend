using System;

namespace EcommerceApi.Controllers
{
    public class UpdatePurchaseDetailStatus
    {
        public decimal Amount { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public string PurchaseStatus { get; set; }
        public DateTime? PaidDate { get; set; }
        public string PoNumber { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
        public int? ArrivedAtLocationId { get; set; }
        public DateTime? ArrivedDate { get; set; }
    }
}