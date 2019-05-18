namespace EcommerceApi.ViewModel
{
    public class ProductProfitReportViewModel
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string ProductTypeName { get; set; }
        public string AvgSalePrice { get; set; }
        public string AvgPurchasePrice { get; set; }
        public string AvgOverheadCost { get; set; }
        public string AvgTotalCost { get; set; }
        public string TotalSales { get; set; }
        public string TotalProfit { get; set; }
        public string TotalPurchase { get; set; }
        public decimal Amount { get; set; }
    }
}
