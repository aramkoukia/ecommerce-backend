namespace EcommerceApi.ViewModel.Website
{
    public class WebsiteProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductTypeName { get; set; }
        public string Balance { get; set; }
        public string ImagePath { get; set; }
        public string ProductDescription { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public string SlugsUrl { get; set; }
        public string[] ImagePaths { get; set; }
    }
}