namespace EcommerceApi.ViewModel
{
    public class PurchasesReportViewModel
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string PlannedAmount { get; set; }
        public string PlannedTotalPrice { get; set; }
        public string PaidAmount { get; set; }
        public string PaidTotalPrice { get; set; }
        public string OnDevliveryAmount { get; set; }
        public string OnDevliveryTotalPrice { get; set; }
        public string CustomClearanceAmount { get; set; }
        public string CustomClearanceTotalPrice { get; set; }
        public string ArrivedAmount { get; set; }
        public string ArrivedPrice { get; set; }
    }
}
