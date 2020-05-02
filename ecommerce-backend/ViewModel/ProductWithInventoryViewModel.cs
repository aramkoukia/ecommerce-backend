using EcommerceApi.Models;
using System;
using System.Collections.Generic;

namespace EcommerceApi.ViewModel
{
    public class ProductWithInventoryViewModel
    {
        public ProductWithInventoryViewModel()
        {
            Inventory = new List<ProductWithInventoryDetail>();
        }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public bool ChargeTaxes { get; set; }
        public bool AllowOutOfStockPurchase { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal Balance { get; set; }
        public string BinCode { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public decimal OnHoldAmount { get; set; }
        public string Disabled { get; set; }
        public List<ProductWithInventoryDetail> Inventory { get; set; }
    }
}
