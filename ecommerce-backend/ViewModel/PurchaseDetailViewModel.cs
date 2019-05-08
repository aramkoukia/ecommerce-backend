using System;

namespace EcommerceApi.ViewModel
{
    public class PurchaseDetailViewModel
    {
        public int PurchaseId { get; set; }
        public string Supplier { get; set; }
        public string PONumber { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal PlanAmount { get; set; }
        public decimal PlanPrice { get; set; }
        public decimal PlanOverheadCost { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidPrice { get; set; }
        public decimal PaidOverheadCost { get; set; }
        public decimal RemainToPay { get; set; }
        public decimal OnDeliveryAmount { get; set; }
        public decimal OnDelivertPrice { get; set; }
        public decimal OnDeliveryOverheadCost { get; set; }
        public decimal CustomClearanceAmount { get; set; }
        public decimal CustomClearancePrice { get; set; }
        public decimal CustomClearanceOverheadCost { get; set; }
        public decimal ArrivedAmount { get; set; }
        public decimal ArrivedPrice { get; set; }
        public decimal ArrivedOverheadCost { get; set; }
        public string LocationName { get; set; }
        public DateTime ArrivedDate { get; set; }
        public decimal RemainToArrive { get; set; }
    }
}
