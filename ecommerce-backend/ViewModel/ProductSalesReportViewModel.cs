namespace EcommerceApi.ViewModel
{
    public class ProductSalesReportViewModel
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string ProductTypeName { get; set; }
        public string VanTotalSales { get; set; }
        public string AbbTotalSales { get; set; }
        public decimal VanBalance { get; set; }
        public decimal AbbBalance { get; set; }
        public decimal VanAmount { get; set; }
        public decimal AbbAmount { get; set; }
    }
}
