using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Tags")]
    public class TagsController : Controller
    {
        private readonly EcommerceContext _context;

        public TagsController(EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/Tags/
        [HttpGet("{name}")]
        public async Task<IActionResult> GetTag([FromRoute] string name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tag = await _context.Tags.SingleOrDefaultAsync(m => m.TagName == name);

            if (tag == null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        // POST: api/Tags
        [HttpPost]
        public async Task<IActionResult> PostTag([FromBody] string name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var tag = new Tag
            {
                TagName = name
            };
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTag", new { id = tag.TagId }, tag);
        }

        private bool TagExists(string name)
        {
            return _context.Tags.Any(e => e.TagName == name);
        }
    }
}