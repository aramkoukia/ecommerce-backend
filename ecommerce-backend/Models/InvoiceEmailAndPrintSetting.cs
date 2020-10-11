namespace EcommerceApi.Models
{
    public class InvoiceEmailAndPrintSetting
    {
        public int Id { get; set; }
        public string EmailSubject { get; set; }
        public string EmailAttachmentFileName { get; set; }
        public string EmailBody { get; set; }
        public string CCEmailAddress { get; set; }
    }
}
