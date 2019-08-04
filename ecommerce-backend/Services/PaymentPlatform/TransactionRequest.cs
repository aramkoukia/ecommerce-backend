namespace EcommerceApi.Services.PaymentPlatform
{
    public class TransactionRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public string ClientIp { get; set; }
    }
}