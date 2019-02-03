using System;

namespace EcommerceApi.Models
{
    public partial class TransferInventory
    {
        public int ProductId { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public string TransferNotes { get; set; }
        public decimal TransferQuantity { get; set; }
    }
}
