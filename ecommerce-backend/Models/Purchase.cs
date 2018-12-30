using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models
{
    public class Purchase
    {
        public Purchase()
        {
            PurchaseDetail = new HashSet<PurchaseDetail>();
        }

        public int? PurchaseId { get; set; }
        public string Supplier { get; set; }
        public int LocationId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime DeliveryDateDate { get; set; }

        [Required]
        [Range(1, 10000000, ErrorMessage = "Purchase total cannot be zero")]
        public decimal Total { get; set; }

        [Required]
        [Range(1, 10000000, ErrorMessage = "Purchase total cannot be zero")]
        public decimal SubTotal { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByUserId { get; set; }

        public ICollection<PurchaseDetail> PurchaseDetail { get; set; }
    }
}
