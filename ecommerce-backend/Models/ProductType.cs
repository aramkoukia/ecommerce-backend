using System;

namespace EcommerceApi.Models
{
    [Serializable]
    public class ProductType
    {
        public ProductType()
        {
        }

        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ThumbnailImagePath { get; set; }
        public string HeaderImagePath { get; set; }
        public string Description { get; set; }
        public string SlugsUrl { get; set; }
        public bool ShowOnWebsite { get; set; }
        public string HeaderImageSize { get; set; }
        public string ThumbnailImageSize { get; set; }
    }
}
