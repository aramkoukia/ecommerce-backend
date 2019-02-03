using System;

namespace EcommerceApi.Models
{
    public partial class ProductInventoryHistory
    {
        public int ProductInventoryHistoryId { get; set; }
        public int ProductId { get; set; }
        public int LocationId { get; set; }
        public decimal Balance { get; set; }
        public string Notes { get; set; }
        public string BinCode { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string TransactionType { get; set; }
    }
}
