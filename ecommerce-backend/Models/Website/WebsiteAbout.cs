namespace EcommerceApi.Models
{
    public partial class WebsiteAbout
    {
        public WebsiteAbout()
        {
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string AboutText { get; set; }
        public int SortOrder { get; set; }
        public string HeaderImagePath { get; set; }
        public string HeaderImageSize { get; set; }
    }
}
