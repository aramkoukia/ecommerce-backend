namespace EcommerceApi.ViewModel.Moneris
{
    public class MonerisAdminRequest
    {
        public string StoreId { get; set; }
        public string TerminalId { get; set; }
        public string ClientIp { get; set; }
        public string UserId { get; set; }
        public string PairingToken { get; set; }
    }
}