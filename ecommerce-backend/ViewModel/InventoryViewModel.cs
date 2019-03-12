namespace EcommerceApi.Repositories
{
    public class InventoryViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string LocationName { get; set; }
        public decimal Balance { get; set; }
        public decimal OnHold { get; set; }
    }
}