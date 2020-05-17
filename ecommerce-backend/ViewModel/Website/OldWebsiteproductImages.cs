namespace EcommerceApi.Controllers
{
    public class Rootobject
    {
        public OldWebsiteproductImage[] Products { get; set; }
    }

    public class OldWebsiteproductImage
    {
        public string text { get; set; }
        public string code { get; set; }
        public string[] image { get; set; }
    }

}
