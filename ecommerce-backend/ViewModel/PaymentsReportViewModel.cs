namespace EcommerceApi.ViewModel
{
    public class PaymentsReportViewModel
    {
        public string GivenName { get; set; }
        public string PaymentTypeName { get; set; }
        public int OrderId { get; set; }
        public decimal PaymentAmount { get; set; }
        public string Status { get; set; }
        public string CompanyName { get; set; }
        public string LocationName { get; set; }
    }
}
