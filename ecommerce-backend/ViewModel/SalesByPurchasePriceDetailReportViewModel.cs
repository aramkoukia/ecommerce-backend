namespace EcommerceApi.ViewModel
{
    public class SalesByPurchasePriceDetailReportViewModel
    {
        public string LocationName { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Amount { get; set; }
        public string SalesPrice { get; set; }
        public string PurchasePrice { get; set; }
        public string TotalBySalePrice { get; set; }
        public string TotalByPurchasePrice { get; set; }
    }
}
