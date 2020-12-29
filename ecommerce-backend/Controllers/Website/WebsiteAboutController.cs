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
    [Route("api/WebsiteAbout")]
    [Authorize()]
    public class WebsiteAboutController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IConfiguration _config;
        private const string ContentContainerName = "websiteabout";

        public WebsiteAboutController(
            EcommerceContext context,
            IConfiguration config
            ) {
            _context = context;
            _config = config;
        }

        // GET: api/Website/Slider
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<WebsiteAbout> GetAsync() =>
          _context.WebsiteAbout.OrderBy(b => b.SortOrder).ToList();

        // PUT: api/WebsiteAbout/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWebsiteSlider([FromRoute] int id, [FromBody] WebsiteAbout websiteAbout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != websiteAbout.Id)
            {
                return BadRequest();
            }

            _context.Entry(websiteAbout).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WebsiteAboutExists(id))
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

        // POST: api/WebsiteAbout
        [HttpPost]
        public async Task<IActionResult> PostWebsiteAbout([FromBody] WebsiteAbout websiteAbout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.WebsiteAbout.Add(websiteAbout);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WebsiteAboutExists(websiteAbout.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetWebsiteAbout", new { id = websiteAbout.Id }, websiteAbout);
        }

        // DELETE: api/WebsiteAbout/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWebsiteAbout([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var websiteAbout = await _context.WebsiteAbout.SingleOrDefaultAsync(m => m.Id == id);
            if (websiteAbout == null)
            {
                return NotFound();
            }
            _context.WebsiteAbout.Remove(websiteAbout);
            await _context.SaveChangesAsync();

            return Ok(websiteAbout);
        }

        [HttpPost]
        [Route("{id}/Upload")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadAsync([FromRoute] int id, IFormFile file)
        {
            var exisintgWebsiteAbout = await _context.WebsiteAbout.FirstOrDefaultAsync(m => m.Id == id);
            if (exisintgWebsiteAbout == null)
            {
                return BadRequest($"BlogPostId {id} not found.");
            }

            var storageConnectionString = _config.GetConnectionString("AzureStorageConnectionString");

            if (!CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(ContentContainerName);

            await container.CreateIfNotExistsAsync();

            //MS: Don't rely on or trust the FileName property without validation. The FileName property should only be used for display purposes.
            var picBlob = container.GetBlockBlobReference(Guid.NewGuid().ToString() + "-" + file.FileName);

            await picBlob.UploadFromStreamAsync(file.OpenReadStream());

            exisintgWebsiteAbout.HeaderImagePath = picBlob.Uri.AbsoluteUri;
            exisintgWebsiteAbout.HeaderImageSize = file.Length.ToString();
            await _context.SaveChangesAsync();
            return Ok(exisintgWebsiteAbout);
        }

        private bool WebsiteAboutExists(int id)
        {
            return _context.WebsiteAbout.Any(e => e.Id == id);
        }
    }
}