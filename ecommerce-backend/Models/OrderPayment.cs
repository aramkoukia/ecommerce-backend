using System;

namespace EcommerceApi.Models
{
    public partial class OrderPayment
    {
        public int OrderPaymentId { get; set; }
        public int? OrderId { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PaymentTypeId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByUserId { get; set; }
        public string AuthCode { get; set; }
        public string CreditCard { get; set; }
        public string Notes { get; set; }
        public string ChequeNo { get; set; }
        public PaymentType PaymentType { get; set; }
    }
}
