using System;

namespace EcommerceApi.ViewModel
{
    public class ProductTransactionViewModel
    {
        public DateTime Date { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string LocationName { get; set; }
        public string UserName { get; set; }
        public string Notes { get; set; }
    }
}
