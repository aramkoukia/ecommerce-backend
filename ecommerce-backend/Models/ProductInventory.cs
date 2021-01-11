using System;

namespace EcommerceApi.Models
{
    [Serializable]
    public class ProductInventory
    {
        public int ProductInventoryId { get; set; }
        public int ProductId { get; set; }
        public int LocationId { get; set; }
        public decimal Balance { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string BinCode { get; set; }
    }
}
