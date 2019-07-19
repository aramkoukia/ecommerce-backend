using System;

namespace EcommerceApi.ViewModel
{
    public class InventoryValueReportViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public decimal VancouverBalance { get; set; }
        public decimal AbbotsfordBalance { get; set; }
        public decimal VancouverOnHold { get; set; }
        public decimal AbbotsfordOnHold { get; set; }
        public decimal OnHoldAmount { get; set; }
        public string Disabled { get; set; }
        public string AvgPurchasePrice { get; set; }
        public string VancouverValue { get; set; }
        public string AbbotsfordValue { get; set; }
        public string TotalValue { get; set; }

    }
}
