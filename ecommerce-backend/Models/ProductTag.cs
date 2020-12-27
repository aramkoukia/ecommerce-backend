namespace EcommerceApi.Models
{
    public class ProductTag
    {
        public int ProductTagId { get; set; }
        public int ProductId { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }

}
