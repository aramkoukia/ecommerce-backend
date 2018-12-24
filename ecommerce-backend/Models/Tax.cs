using System;

namespace EcommerceApi.Models
{
    public partial class Tax
    {
        public int TaxId { get; set; }
        public string TaxName { get; set; }
        public decimal Percentage { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
