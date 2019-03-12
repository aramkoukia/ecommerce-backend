using System.Collections.Generic;

namespace EcommerceApi.ViewModel
{
    public class InventoryValidationRequest
    {
        public List<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        public int LocationId { get; set; }
        public int ProductId { get; set; }
        public decimal Amount { get; set; }
    }
}
