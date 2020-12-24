using System.Collections.Generic;

namespace EcommerceApi.ViewModel.Website
{
    public class WebsiteProductsInCategoryViewModel
    {
        public WebsiteProductsInCategoryViewModel()
        {
            Images = new List<ProductImage>();
        }

        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductTypeName { get; set; }
        public string Balance { get; set; }
        public string ProductDescription { get; set; }
        public int Rank { get; set; }
        public string SlugsUrl { get; set; }
        public List<ProductImage> Images { get; set; }
    }
}