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
using System.Web;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/websitepage")]
    [Authorize()]
    public class WebsitePageController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IConfiguration _config;
        private const string ContentContainerName = "websitepage";

        public WebsitePageController(
            EcommerceContext context,
            IConfiguration config
            ) {
            _context = context;
            _config = config;
        }

        // GET: api/Website/Slider
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<WebsitePage> GetAsync() =>
          _context.WebsitePage.ToList();

        [HttpGet("{url}")]
        [AllowAnonymous]
        public WebsitePage GetAsync(string url) =>
          _context.WebsitePage.FirstOrDefault(p => p.Url.ToLower() == HttpUtility.UrlDecode(url).ToLower());

        // PUT: api/WebsitePage/5
        [HttpPut("{url}")]
        public async Task<IActionResult> PutWebsiteSlider([FromRoute] string url, [FromBody] WebsitePage websitePage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (url != websitePage.Url)
            {
                return BadRequest();
            }

            _context.Entry(websitePage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WebsitePageExists(url))
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

        // POST: api/WebsitePage
        [HttpPost]
        public async Task<IActionResult> PostWebsitePage([FromBody] WebsitePage websitePage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.WebsitePage.Add(websitePage);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WebsitePageExists(websitePage.Url))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return Ok(websitePage);
        }

        // DELETE: api/WebsitePage/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWebsitePage([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var websitePage = await _context.WebsitePage.SingleOrDefaultAsync(m => m.Id == id);
            if (websitePage == null)
            {
                return NotFound();
            }
            _context.WebsitePage.Remove(websitePage);
            await _context.SaveChangesAsync();

            return Ok(websitePage);
        }

        [HttpPost]
        [Route("{encodedUrl}/Upload")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadAsync([FromRoute] string encodedUrl, IFormFile file)
        {
            var url = HttpUtility.UrlDecode(encodedUrl);
            var exisintgWebsitePage = await _context.WebsitePage.FirstOrDefaultAsync(m => m.Url == url);
            if (exisintgWebsitePage == null)
            {
                return BadRequest($"WebSitePage {url} not found.");
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

            exisintgWebsitePage.HeaderImagePath = picBlob.Uri.AbsoluteUri;
            exisintgWebsitePage.HeaderImageSize = file.Length.ToString();
            await _context.SaveChangesAsync();
            return Ok(exisintgWebsitePage);
        }

        private bool WebsitePageExists(string url)
        {
            return _context.WebsitePage.Any(e => e.Url == url);
        }
    }
}