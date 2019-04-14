using System;

namespace EcommerceApi.ViewModel
{
    public class PurchaseViewModel
    {
        public int PurchaseId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public string Supplier { get; set; }
        public string PoNumber { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string CreatedByUserId { get; set; }
    }
}
