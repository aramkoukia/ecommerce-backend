namespace EcommerceApi.ViewModel.Moneris
{
    public class MonerisRequest
    {
        public string StoreId { get; set; }
        public string ApiToken { get; set; }
        public string TerminalId { get; set; }
        public string TxnType { get; set; }
        public string PostbackUrl { get; set; }
    }
}