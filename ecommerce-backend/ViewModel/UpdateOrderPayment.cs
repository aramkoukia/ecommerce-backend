using EcommerceApi.Models;
using System.Collections.Generic;

namespace EcommerceApi.Controllers
{
    public class UpdateOrderPayment
    {
        public UpdateOrderPayment()
        {
            OrderPayment = new HashSet<OrderPayment>();
        }

        public ICollection<OrderPayment> OrderPayment { get; set; }
    }
}