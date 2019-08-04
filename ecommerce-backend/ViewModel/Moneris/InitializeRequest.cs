namespace EcommerceApi.ViewModel.Moneris
{
    namespace EcommerceApi.ViewModel.Moneris
    {
        public class InitializeRequest : MonerisRequest
        {
            public RequestDetail Request { get; set; }
            public class RequestDetail
            {
                public string PairingToken { get; set; }
            }
        }
    }

}