using System;

namespace EcommerceApi.Models.Moneris
{
    public class MonerisTransactionLog
    {
        public int Id { get; set; }
        public string ClientIp { get; set; }
        public string StoreId { get; set; }
        public string TerminalId { get; set; }
        public string UserId { get; set; }
        public string Request { get; set; }
        public string TransactionType { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string Response { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}