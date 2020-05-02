namespace EcommerceApi.ViewModel
{
    public class ProductWithInventoryDetail
    {
        public int ProductId { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public decimal Balance { get; set; }
        public string BinCode { get; set; }
        public decimal OnHoldAmount { get; set; }
    }
}
