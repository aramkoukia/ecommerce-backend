using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models
{
    public partial class PurchaseDetail
    {
        public int PurchaseDetailId { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public decimal Amount { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? OverheadCost { get; set; }
        public decimal? TotalPrice { get; set; }
        public string Status { get; set; }
        public string PoNumber { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
        public int? ArrivedAtLocationId { get; set; }
        public DateTime? ArrivedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByUserId { get; set; }

        public Purchase Purchase { get; set; }
        public Product Product { get; set; }

        [ForeignKey("ArrivedAtLocationId")]
        public Location Location { get; set; }
    }
}
