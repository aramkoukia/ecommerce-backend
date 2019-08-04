namespace EcommerceApi.ViewModel.Moneris
{
    public class MonerisRequest
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
        public string PairingToken { get; set; }
    }
}
