using EcommerceApi.Models;
using System.Collections.Generic;

namespace EcommerceApi.Controllers
{
    public class UpdateOrderStatus
    {
        public UpdateOrderStatus()
        {
            OrderPayment = new HashSet<OrderPayment>();
        }

        public string OrderStatus { get; set; }
        public ICollection<OrderPayment> OrderPayment { get; set; }
    }
}