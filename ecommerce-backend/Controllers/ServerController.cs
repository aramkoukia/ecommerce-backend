using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EcommerceApi.Controllers
{
    [Route("/")]
    public class ServerController : Controller
    {
        [HttpGet("robots.txt")]
        public ContentResult GetRobotsFile()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("user-agent: *");
            stringBuilder.AppendLine("disallow: *");

            return Content(stringBuilder.ToString(), "text/plain", Encoding.UTF8);
        }
    }
}