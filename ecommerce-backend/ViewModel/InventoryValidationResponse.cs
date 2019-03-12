namespace EcommerceApi.ViewModel
{
    public class InventoryValidationResponse
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string LocationName { get; set; }
        public decimal Amount { get; set; }
        public decimal OnHold { get; set; }
        public decimal AmountRequested { get; set; }
        public decimal AmountShort { get; set; }
    }
}
