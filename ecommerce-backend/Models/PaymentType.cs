using System;

namespace EcommerceApi.Models
{
    [Serializable]
    public class PaymentType
    {
        public int PaymentTypeId { get; set; }
        public string PaymentTypeName { get; set; }
    }
}
