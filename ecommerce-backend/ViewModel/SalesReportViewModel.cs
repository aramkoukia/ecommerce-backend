namespace EcommerceApi.ViewModel
{
    public class SalesReportViewModel
    {
        public string LocationName { get; set; }
        public string Status { get; set; }
        public string SubTotal { get; set; }
        public string Total { get; set; }
        public string Discount { get; set; }
        public int Transactions { get; set; }
        public string Pst { get; set; }
        public string Gst { get; set; }
        public string OtherTax { get; set; }
    }
}
