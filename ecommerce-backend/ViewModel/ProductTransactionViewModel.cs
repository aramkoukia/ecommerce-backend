using System;

namespace EcommerceApi.ViewModel
{
    public class ProductTransactionViewModel
    {
        public DateTime Date { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string Location { get; set; }
        public string User { get; set; }
        public string Customer { get; set; }
    }
}
