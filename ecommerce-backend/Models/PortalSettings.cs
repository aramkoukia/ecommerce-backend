namespace EcommerceApi.Models
{
    public class PortalSettings
    {
        public int Id { get; set; }
        public string PortalTitle { get; set; }
        public string SidebarImageUrl { get; set; }
        public string LogoImageUrl { get; set; }
        public bool ShowTitleOnSideBar { get; set; }
        public string LegalName { get; set; }
        public string GstNumber { get; set; }
        public string InvoicePhone { get; set; }
        public string InvoiceLogoImage { get; set; }
        public string InvoiceWebsite { get; set; }
        public string WebsiteLogoUrl { get; set; }
        public string WebsiteFavIconUrl { get; set; }
        public string PublicWebsiteUrl { get; set; }
    }
}
