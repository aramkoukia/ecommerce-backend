namespace EcommerceApi.ViewModel
{
    public class SalesReportViewModel
    {
        public string LocationName { get; set; }
        public string Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public int Transactions { get; set; }
        public decimal Pst { get; set; }
        public decimal Gst { get; set; }
        public decimal OtherTax { get; set; }
    }
}
