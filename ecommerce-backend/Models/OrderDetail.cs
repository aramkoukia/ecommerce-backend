namespace EcommerceApi.Models
{
    public partial class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal Amount { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
