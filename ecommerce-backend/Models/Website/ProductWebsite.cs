namespace EcommerceApi.Models
{
    public partial class ProductWebsite
    {
        public ProductWebsite()
        {
        }

        public int ProductWebsiteId { get; set; }
        public int ProductId { get; set; }
        public string SlugsUrl { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public string WarrantyInformation { get; set; }
        public string AdditionalInformation { get; set; }
        public string UserManualPath { get; set; }
    }
}
