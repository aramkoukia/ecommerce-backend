using System;

namespace EcommerceApi.ViewModel
{
    public class PurchasesDetailReportViewModel
    {
        public int PurchaseId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string Supplier { get; set; }
        public string Status { get; set; }
        public string Amount { get; set; }
        public string UnitPrice { get; set; }
        public string TotalPrice { get; set; }
        public string PONumber { get; set; }
        public DateTime EstimatedDelivery { get; set; }
        public DateTime PaidDate { get; set; }
        public DateTime ArrivedDate { get; set; }
        public string LocationName { get; set; }
    }
}
