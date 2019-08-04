namespace EcommerceApi.ViewModel.Moneris
{
    public class PairRequest
    {

        public string StoreId { get; set; }
        public string ApiToken { get; set; }
        public string TerminalId { get; set; }
        public string TxnType { get; set; }
        public string PostbackUrl { get; set; }
        public RequestDetail Request { get; set; }

        public class RequestDetail
        {
            public string PairingToken { get; set; }
        }
    }
}
