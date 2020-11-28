using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models.Website;
using EcommerceApi.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Website")]
    [Authorize()]
    public class WebsiteSliderController : Controller
    {
        private readonly EcommerceContext _context;

        public WebsiteSliderController(EcommerceContext context) =>
          _context = context;

        // GET: api/Website/Slider
        [HttpGet("Slider")]
        public IEnumerable<WebsiteSlider> GetAsync() =>
          _context.WebsiteSlider.ToList();

    }
}