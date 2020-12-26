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
        [HttpGet]
        public async Task<IActionResult> GetTags()
        {
            var tags = await _context.Tag.ToListAsync();
            return Ok(tags);
        }

        // POST: api/Tags
        [HttpPost]
        public async Task<IActionResult> PostTag([FromBody] Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Tag.Count() == 0)
            {
                tag.TagId = 1;
            }
            else
            {
                tag.TagId = _context.Tag.Max(l => l.TagId) + 1;
            }

            _context.Tag.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTag", new { id = tag.TagId }, tag);
        }

        // PUT: api/tags/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTag([FromRoute] int id, [FromBody] Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tag.TagId)
            {
                return BadRequest();
            }

            _context.Entry(tag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tag = await _context.Tag.SingleOrDefaultAsync(m => m.TagId == id);
            if (tag == null)
            {
                return NotFound();
            }
            _context.Tag.Remove(tag);
            await _context.SaveChangesAsync();

            return Ok(tag);
        }

        private bool TagExists(int id)
        {
            return _context.Tag.Any(e => e.TagId == id);
        }
    }
}