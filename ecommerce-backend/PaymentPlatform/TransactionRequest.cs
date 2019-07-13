namespace EcommerceApi.PaymentPlatform
{
    public class TransactionRequest
    {
        public string StoreId { get; set; }
        public string ApiToken { get; set; }
        public string TerminalId { get; set; }
        public string TxnType { get; set; }
        public string PostbackUrl { get; set; }
        public Request Request { get; set; }
    }

    public class Request
    {
        public string OrderId { get; set; }
        public string Amount { get; set; }
    }
}