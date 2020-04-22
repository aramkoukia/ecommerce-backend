using EcommerceApi.Models;
using System;
using System.Collections.Generic;

namespace EcommerceApi.ViewModel
{
    public class ProductViewModelV2
    {
        public ProductViewModelV2()
        {
            ProductPackages = new List<ProductPackage>();
        }

        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public bool ChargeTaxes { get; set; }
        public bool AllowOutOfStockPurchase { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public decimal Balance { get; set; }
        public string BonCode { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public decimal OnHoldAmount { get; set; }
        public string Disabled { get; set; }
        public string AvgPurchasePrice { get; set; }
        public List<ProductPackage> ProductPackages { get; set; }
    }
}
