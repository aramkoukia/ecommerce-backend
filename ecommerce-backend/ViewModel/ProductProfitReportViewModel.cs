namespace EcommerceApi.ViewModel
{
    public class ProductProfitReportViewModel
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string ProductTypeName { get; set; }
        public string PurchaseAmount { get; set; }
        public string AvgPurchasePrice { get; set; }
        public string AvgOverheadCost { get; set; }
        public string AvgTotalCost { get; set; }
        public string SalesAmount { get; set; }
        public string TotalSales { get; set; }
        public string AvgSalesPrice { get; set; }
        public string TotalCost { get; set; }
        public string AvgProfitPerItem { get; set; }
        public string TotalProfit { get; set; }
    }
}
