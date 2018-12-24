using System;
using System.Collections.Generic;

namespace EcommerceApi.Models
{
    public partial class OrderTax
    {
        public int OrderTaxId { get; set; }
        public int OrderId { get; set; }
        public int TaxId { get; set; }
        public decimal TaxAmount { get; set; }

        public Order Order { get; set; }
    }
}
