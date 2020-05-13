using System;

namespace EcommerceApi.Models
{
    public partial class ProductType
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
    }
}
