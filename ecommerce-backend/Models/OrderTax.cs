using System;

namespace EcommerceApi.Models
{
    [Serializable]
    public class OrderTax
    {
        public int OrderTaxId { get; set; }
        public int OrderId { get; set; }
        public int TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public Order Order { get; set; }
        public Tax Tax { get; set; }
    }
}
