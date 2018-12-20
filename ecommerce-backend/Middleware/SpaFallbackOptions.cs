namespace EcommerceApi.Middleware
{
 public class SpaFallbackOptions
    {
        public SpaFallbackOptions()
        {
            ApiPathPrefix = "/api";
            RewritePath = "/";
        }
        public string ApiPathPrefix { get; set; }
        public string RewritePath { get; set; }
    }
}
