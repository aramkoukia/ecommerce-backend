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
    [Route("api/BlogPosts")]
    [Authorize()]
    public class BlogPostController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IConfiguration _config;
        private const string ContentContainerName = "blogposts";

        public BlogPostController(
            EcommerceContext context,
            IConfiguration config
            ) {
            _context = context;
            _config = config;
        }

        // GET: api/Website/Slider
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<BlogPost> GetAsync() =>
          _context.BlogPost.OrderByDescending(b => b.CreatedDate).ToList();

        [HttpGet("Recent")]
        [AllowAnonymous]
        public IEnumerable<BlogPost> GetRecentAsync() =>
           _context.BlogPost.OrderByDescending(b => b.CreatedDate).ToList().Take(5);

        // PUT: api/BlogPosts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWebsiteSlider([FromRoute] int id, [FromBody] BlogPost blogPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != blogPost.Id)
            {
                return BadRequest();
            }

            _context.Entry(blogPost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogPostExists(id))
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

        // POST: api/BlogPost
        [HttpPost]
        public async Task<IActionResult> PostBlog([FromBody] BlogPost blogPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.BlogPost.Add(blogPost);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BlogPostExists(blogPost.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBlogPost", new { id = blogPost.Id }, blogPost);
        }

        // DELETE: api/BlogPosts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlogPost([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var blogPost = await _context.BlogPost.SingleOrDefaultAsync(m => m.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }
            _context.BlogPost.Remove(blogPost);
            await _context.SaveChangesAsync();

            return Ok(blogPost);
        }

        [HttpPost]
        [Route("{id}/Upload")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadAsync([FromRoute] int id, IFormFile file)
        {
            var exisintgBlogPost = await _context.BlogPost.FirstOrDefaultAsync(m => m.Id == id);
            if (exisintgBlogPost == null)
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

            exisintgBlogPost.SmallImagePath = picBlob.Uri.AbsoluteUri;
            await _context.SaveChangesAsync();
            return Ok(exisintgBlogPost);
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPost.Any(e => e.Id == id);
        }
    }
}