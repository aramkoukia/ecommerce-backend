using System;
using System.Collections.Generic;

namespace EcommerceApi.Models
{
    public partial class ProductType
    {
        public ProductType()
        {
            // Product = new HashSet<Product>();
        }

        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public DateTime ModifiedDate { get; set; }

        // public ICollection<Product> Product { get; set; }
    }
}
