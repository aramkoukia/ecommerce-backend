namespace EcommerceApi.ViewModel
{
    public class SalesForecastReportViewModel
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Last1Month { get; set; }
        public string Last3Month { get; set; }
        public string Last6Month { get; set; }
        public string Last12Month { get; set; }
        public string Last12MonthAverage { get; set; }
        public string Balance { get; set; }
        public string NeedsPurchase { get; set; }
    }
}
