namespace EcommerceApi.ViewModel
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public int LocationId { get; set; }
        public string OrderDate { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public int DiscountId { get; set; }
        public string PstNumber { get; set; }
        public string Notes { get; set; }
        public string PoNumber { get; set; }
        public string Status { get; set; }
        public string GivenName { get; set; }
        public decimal PaidAmount { get; set; }
        public string LocationName { get; set; }
        public string PaymentTypeName { get; set; }
        public string CompanyName { get; set; }
    }
}
