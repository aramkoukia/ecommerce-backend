using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models.Website;
using EcommerceApi.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using System;
using Microsoft.Extensions.Configuration;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/WebsiteAboutPopOverPopOver")]
    [Authorize()]
    public class WebsiteAboutPopOverController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IConfiguration _config;
        private const string ContentContainerName = "websiteabout";

        public WebsiteAboutPopOverController(
            EcommerceContext context,
            IConfiguration config
            ) {
            _context = context;
            _config = config;
        }

        // GET: api/Website/Slider
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<WebsiteAboutPopOver> GetAsync() =>
          _context.WebsiteAboutPopOver.ToList();

        // PUT: api/WebsiteAboutPopOver/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWebsiteSlider([FromRoute] int id, [FromBody] WebsiteAboutPopOver websiteAboutPopOver)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != websiteAboutPopOver.Id)
            {
                return BadRequest();
            }

            _context.Entry(websiteAboutPopOver).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WebsiteAboutPopOverExists(id))
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

        // POST: api/WebsiteAboutPopOver
        [HttpPost]
        public async Task<IActionResult> PostWebsiteAboutPopOver([FromBody] WebsiteAboutPopOver websiteAboutPopOver)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.WebsiteAboutPopOver.Add(websiteAboutPopOver);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WebsiteAboutPopOverExists(websiteAboutPopOver.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetWebsiteAboutPopOver", new { id = websiteAboutPopOver.Id }, websiteAboutPopOver);
        }

        // DELETE: api/WebsiteAboutPopOver/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWebsiteAboutPopOver([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var websiteAboutPopOver = await _context.WebsiteAboutPopOver.SingleOrDefaultAsync(m => m.Id == id);
            if (websiteAboutPopOver == null)
            {
                return NotFound();
            }
            _context.WebsiteAboutPopOver.Remove(websiteAboutPopOver);
            await _context.SaveChangesAsync();

            return Ok(websiteAboutPopOver);
        }

        private bool WebsiteAboutPopOverExists(int id)
        {
            return _context.WebsiteAboutPopOver.Any(e => e.Id == id);
        }
    }
}