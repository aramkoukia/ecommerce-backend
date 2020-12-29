namespace EcommerceApi.Models
{
    public partial class WebsiteFaq
    {
        public WebsiteFaq()
        {
        }

        public int Id { get; set; }
        public string Section { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public int SortOrder { get; set; }
        public string HeaderImagePath { get; set; }
        public string HeaderImageSize { get; set; }
    }
}
