using System;

namespace EcommerceApi.Services.PaymentPlatform
{
    public class TransactionRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public string ClientIp { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}