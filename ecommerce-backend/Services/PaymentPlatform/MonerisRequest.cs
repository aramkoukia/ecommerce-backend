namespace EcommerceApi.Services.PaymentPlatform
{
    public class MonerisRequest
    {
        public string storeId { get; set; }
        public string apiToken { get; set; }
        public string terminalId { get; set; }
        public string txnType { get; set; }
        public string postbackUrl { get; set; }
        public Request request { get; set; }
    }

    public class Request
    {
        public string orderId { get; set; }
        public string amount { get; set; }
    }

    public enum TransactionType
    {
        pairing,
        initialize,
        purchase,
        refund
    }
}