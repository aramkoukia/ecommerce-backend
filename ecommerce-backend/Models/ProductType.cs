using System;
using System.Collections.Generic;

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
        public DateTime ThumbnailImagePath { get; set; }
        public string HeaderImagePath { get; set; }
        public string Description { get; set; }
    }
}
