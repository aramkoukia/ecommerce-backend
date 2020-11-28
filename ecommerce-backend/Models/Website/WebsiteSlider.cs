namespace EcommerceApi.Models.Website
{
    public partial class WebsiteSlider
    {
        public WebsiteSlider()
        {
        }
        public int Id { get; set; }
        public string SubTitle { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public string BgImage { get; set; }
        public string BgColor { get; set; }
    }
}
