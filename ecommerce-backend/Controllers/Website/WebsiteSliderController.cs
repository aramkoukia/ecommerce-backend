using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models.Website;
using EcommerceApi.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/WebsiteSlider")]
    [Authorize()]
    public class WebsiteSliderController : Controller
    {
        private readonly EcommerceContext _context;

        public WebsiteSliderController(EcommerceContext context) =>
          _context = context;

        // GET: api/Website/Slider
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<WebsiteSlider> GetAsync() =>
          _context.WebsiteSlider.ToList();

        // PUT: api/WebsiteSliders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWebsiteSlider([FromRoute] int id, [FromBody] WebsiteSlider WebsiteSlider)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != WebsiteSlider.Id)
            {
                return BadRequest();
            }

            _context.Entry(WebsiteSlider).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WebsiteSliderExists(id))
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

        // POST: api/WebsiteSliders
        [HttpPost]
        public async Task<IActionResult> PostWebsiteSlider([FromBody] WebsiteSlider WebsiteSlider)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.WebsiteSlider.Add(WebsiteSlider);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WebsiteSliderExists(WebsiteSlider.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetWebsiteSlider", new { id = WebsiteSlider.Id }, WebsiteSlider);
        }

        // DELETE: api/WebsiteSliders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWebsiteSlider([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var websiteSlider = await _context.WebsiteSlider.SingleOrDefaultAsync(m => m.Id == id);
            if (websiteSlider == null)
            {
                return NotFound();
            }
            _context.WebsiteSlider.Remove(websiteSlider);
            await _context.SaveChangesAsync();

            return Ok(websiteSlider);
        }

        private bool WebsiteSliderExists(int id)
        {
            return _context.WebsiteSlider.Any(e => e.Id == id);
        }
    }
}