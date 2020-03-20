using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Health")]
    public class HealthController : Controller
    {
        // GET: api/Locations
        [HttpGet]
        public string Get() => "Ping";
    }
}