using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/custom-applications")]
    [Authorize]
    public class CustomApplicationsController : Controller
    {
        private readonly EcommerceContext _context;

        public CustomApplicationsController(EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/Locations
        [HttpGet]
        public IEnumerable<Location> Get()
        {
            return _context.Location;
        }

        // GET: api/Locations/5
        [HttpGet("{id}/steps")]
        public async Task<IActionResult> GetSteps([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var location = await _context.Location.SingleOrDefaultAsync(m => m.LocationId == id);

            if (location == null)
            {
                return NotFound();
            }

            return Ok(location);
        }
    }
}