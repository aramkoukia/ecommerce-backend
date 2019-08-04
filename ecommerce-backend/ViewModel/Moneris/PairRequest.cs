namespace EcommerceApi.ViewModel.Moneris
{
    public class PairRequest : MonerisRequest
    {
        public RequestDetail Request { get; set; }
        public class RequestDetail
        {
            public string PairingToken { get; set; }
        }
    }
}
