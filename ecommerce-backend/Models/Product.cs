using System;
using System.Collections.Generic;

namespace EcommerceApi.Models
{
    public partial class Product
    {
        public Product()
        {
            ProductInventory = new HashSet<ProductInventory>();
            ProductPackage = new HashSet<ProductPackage>();
            ProductType = new ProductType();
        }

        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string Barcode { get; set; }
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int? ProductTypeId { get; set; }
        public bool ChargeTaxes { get; set; }
        public bool AllowOutOfStockPurchase { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public DateTime ModifiedDate { get; set; }
        public ProductType ProductType { get; set; }
        public bool Disabled { get; set; }
        public ICollection<ProductInventory> ProductInventory { get; set; }
        public ICollection<ProductPackage> ProductPackage { get; set; }
    }
}
