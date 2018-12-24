using System;
using System.Collections.Generic;

namespace EcommerceApi.Models
{
    public partial class OrderPayment
    {
        public int OrderPaymentId { get; set; }
        public int? OrderId { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PaymentTypeId { get; set; }
        public int CreatoreUserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedByUserId { get; set; }
    }
}
