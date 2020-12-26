namespace EcommerceApi.Models
{
    public partial class ProductWebsiteImage
    {
        public ProductWebsiteImage()
        {
        }
        public int ProductWebsiteImageId { get; set; }
        public int ProductId { get; set; }
        public string ImagePath { get; set; }
        public string ImageSize { get; set; }
    }
}
