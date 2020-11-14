namespace EcommerceApi.Models
{
    public class InvoiceEmailAndPrintSetting
    {
        public int Id { get; set; }
        public string EmailSubject { get; set; }
        public string EmailAttachmentFileName { get; set; }
        public string EmailBody { get; set; }
        public string CCEmailAddress { get; set; }
        public string PayNote1 { get; set; }
        public string PayNote2 { get; set; }
        public string PayNote3 { get; set; }
        public string Attention { get; set; }
        public string StorePolicy { get; set; }
        public string Footer1 { get; set; }
        public string Signature { get; set; }
        public string AdditionalChargesNote { get; set; }
    }
}
