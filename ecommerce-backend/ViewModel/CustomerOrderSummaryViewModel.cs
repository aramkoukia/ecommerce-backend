namespace EcommerceApi.ViewModel
{
    public class CustomerOrderSummaryViewModel
    {
        public int CustomerId { get; set; }
        public string OrderCount { get; set; }
        public string Status { get; set; }
        public string OrderTotal { get; set; }
        public string OrderSubTotal { get; set; }
    }
}
