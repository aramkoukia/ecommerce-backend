namespace EcommerceApi.Services.PaymentPlatform
{
    public class ValidationResponse
    {
        public Receipt Receipt { get; set; }
    }

    public class Receipt
    {
        public string Completed { get; set; }
        public string TransType { get; set; }
        public string Error { get; set; }
        public string InitRequired { get; set; }
        public string SafIndicator { get; set; }
        public string ResponseCode { get; set; }
        public string ISO { get; set; }
        public string LanguageCode { get; set; }
        public string PartialAuthAmount { get; set; }
        public string AvailableBalance { get; set; }
        public string TipAmount { get; set; }
        public string EMVCashbackAmount { get; set; }
        public string SurchargeAmount { get; set; }
        public string ForeignCurrencyAmount { get; set; }
        public string ForeignCurrencyCode { get; set; }
        public string BaseRate { get; set; }
        public string ExchangeRate { get; set; }
        public string Pan { get; set; }
        public string CardType { get; set; }
        public string CardName { get; set; }
        public string AccountType { get; set; }
        public string SwipeIndicator { get; set; }
        public string FormFactor { get; set; }
        public string CvmIndicator { get; set; }
        public string ReservedField1 { get; set; }
        public string ReservedField2 { get; set; }
        public string AuthCode { get; set; }
        public string EMVEchoData { get; set; }
        public string ReservedField3 { get; set; }
        public string ReservedField4 { get; set; }
        public string Aid { get; set; }
        public string AppLabel { get; set; }
        public string AppPreferredName { get; set; }
        public string Arqc { get; set; }
        public string TvrArqc { get; set; }
        public string Tcacc { get; set; }
        public string TvrTcacc { get; set; }
        public string Tsi { get; set; }
        public string TokenResponseCode { get; set; }
        public string Token { get; set; }
        public string LogonRequired { get; set; }
        public string EncryptedCardInfo { get; set; }
        public string TransDate { get; set; }
        public string TransTime { get; set; }
        public string Amount { get; set; }
        public string ReferenceNumber { get; set; }
        public string ReceiptId { get; set; }
        public string TransId { get; set; }
        public string Timeout { get; set; }
        public string CloudTicket { get; set; }
        public string TxnName { get; set; }
    }
}