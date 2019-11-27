using System;

namespace EcommerceApi.ViewModel
{
    public class InventoryValueTotalByCategoryReportViewModel
    {
        public string LocationName { get; set; }
        public string CategoryName { get; set; }
        public string ValueBySalePrice { get; set; }
        public string ValueByPurchasePrice { get; set; }
    }
}
