using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/websitecontact")]
    [AllowAnonymous()]
    public class WebsiteContactController : Controller
    {
        private readonly EcommerceContext _context;
        
        public WebsiteContactController(
            EcommerceContext context
            ) {
            _context = context;
        }

        // GET: api/Website/Contact
        [HttpGet]
        public IEnumerable<Location> GetAsync() =>
          _context.Location.Where(l => !l.Disabled && l.ShowOnInvoice).ToList();

    }
}