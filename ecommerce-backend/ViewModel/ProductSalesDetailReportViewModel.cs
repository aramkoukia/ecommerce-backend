namespace EcommerceApi.ViewModel
{
    public class ProductSalesDetailReportViewModel
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string CustomerCode { get; set; }
        public string CompanyName { get; set; }
        public string LocationName { get; set; }
        public int OrderId { get; set; }
        public string ProductTypeName { get; set; }
        public string TotalSales { get; set; }
        public decimal Amount { get; set; }
    }
}
