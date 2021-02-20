namespace EcommerceApi.Models
{
    public class WebsitePage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string HeaderImagePath { get; set; }
        public string HeaderImageSize { get; set; }
        public bool ShowOnHeader { get; set; }
        public bool ShowOnFooter { get; set; }
    }
}
