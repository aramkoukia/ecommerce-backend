using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/WebsiteFaq")]
    [Authorize()]
    public class WebsiteFaqController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IConfiguration _config;
        private const string ContentContainerName = "websitefaq";

        public WebsiteFaqController(
            EcommerceContext context,
            IConfiguration config
            ) {
            _context = context;
            _config = config;
        }

        // GET: api/Website/Faq
        [HttpGet]
        public IEnumerable<WebsiteFaq> GetAsync() =>
          _context.WebsiteFaq.OrderBy(b => b.SortOrder).ToList();

        [HttpGet("Formatted")]
        [AllowAnonymous]
        public IEnumerable<WebsiteFaqModel> GetFormattedAsync() =>
            _context.WebsiteFaq.ToList().Select(w => w.Section).Distinct()
                .Select(s=> new WebsiteFaqModel { 
                     Section = s,
                     Questions = new List<WebsiteFaqDetailModel> (
                        _context.WebsiteFaq.Where(w=>w.Section == s).OrderBy(o => o.SortOrder).ToList().AsEnumerable()
                          .Select(d => new WebsiteFaqDetailModel {
                             Question = d.Question,
                             Answer = d.Answer
                          }) 
                     )
                }).ToList();


        // PUT: api/WebsiteFaq/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWebsiteFaq([FromRoute] int id, [FromBody] WebsiteFaq websiteFaq)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != websiteFaq.Id)
            {
                return BadRequest();
            }

            _context.Entry(websiteFaq).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WebsiteFaqExists(id))
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

        // POST: api/WebsiteFaq
        [HttpPost]
        public async Task<IActionResult> PostWebsiteFaq([FromBody] WebsiteFaq websiteFaq)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.WebsiteFaq.Add(websiteFaq);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WebsiteFaqExists(websiteFaq.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetWebsiteFaq", new { id = websiteFaq.Id }, websiteFaq);
        }

        // DELETE: api/WebsiteFaq/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWebsiteFaq([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var websiteFaq = await _context.WebsiteFaq.SingleOrDefaultAsync(m => m.Id == id);
            if (websiteFaq == null)
            {
                return NotFound();
            }
            _context.WebsiteFaq.Remove(websiteFaq);
            await _context.SaveChangesAsync();

            return Ok(websiteFaq);
        }

        [HttpPost]
        [Route("{id}/Upload")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadAsync([FromRoute] int id, IFormFile file)
        {
            var exisintgWebsiteFaq = await _context.WebsiteFaq.FirstOrDefaultAsync(m => m.Id == id);
            if (exisintgWebsiteFaq == null)
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

            await _context.SaveChangesAsync();
            return Ok(exisintgWebsiteFaq);
        }

        private bool WebsiteFaqExists(int id)
        {
            return _context.WebsiteFaq.Any(e => e.Id == id);
        }
    }
}