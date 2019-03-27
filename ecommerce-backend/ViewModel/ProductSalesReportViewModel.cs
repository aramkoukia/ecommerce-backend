namespace EcommerceApi.ViewModel
{
    public class ProductSalesReportViewModel
    {
        public string LocationName { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string ProductTypeName { get; set; }
        public string TotalSales { get; set; }
        public string AbbTotalSales { get; set; }
        public decimal Balance { get; set; }
        public decimal Amount { get; set; }
        public decimal OnHold { get; set; }
    }
}
