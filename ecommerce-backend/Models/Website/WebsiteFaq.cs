using System.Collections.Generic;

namespace EcommerceApi.Models
{
    public class WebsiteFaq
    {
        public int Id { get; set; }
        public string Section { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public int SortOrder { get; set; }
    }

    public class WebsiteFaqModel
    {
        public string Section { get; set; }
        public List<WebsiteFaqDetailModel> Questions { get; set; }
    }

    public class WebsiteFaqDetailModel
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public int SortOrder { get; set; }
    }
}
