using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using EcommerceApi.Repositories;
using EcommerceApi.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/custom-applications")]
    [Authorize]
    public class CustomApplicationsController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IConfiguration _config;
        private readonly ICustomApplicationRepository _customApplicationRepository;
        private const string CustomSolutionsContainerName = "customsolutions";

        public CustomApplicationsController(
            EcommerceContext context,
            ICustomApplicationRepository customApplicationRepository,
            IConfiguration config
            )
        {
            _context = context;
            _customApplicationRepository = customApplicationRepository;
            _config = config;
        }

        // GET: api/custom-applications
        [HttpGet]
        public async Task<IEnumerable<CustomApplicationViewModel>> Get()
            => await _customApplicationRepository.GetCustomApplicationSteps();

        [HttpGet("steps/{id}/step-details")]
        public async Task<IActionResult> GetSteps([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var stepDetails = await _context.ApplicationStepDetail.Where(m => m.ApplicationStepId == id).ToListAsync();
            return Ok(stepDetails);
        }

        [HttpDelete("steps/{id}")]
        public async Task<IActionResult> DeleteStep([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var applicationStep = await _context.ApplicationStep.SingleOrDefaultAsync(m => m.ApplicationStepId == id);
            if (applicationStep == null)
            {
                return NotFound();
            }

            _context.ApplicationStep.Remove(applicationStep);
            await _context.SaveChangesAsync();

            return Ok(applicationStep);
        }

        [HttpPost("steps")]
        public async Task<IActionResult> PostStep([FromBody] ApplicationStep step)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            step.ApplicationStepId = _context.ApplicationStep.Max(l => l.ApplicationStepId) + 1;
            step.IsRangeValue = false;
            step.MaxValue = (decimal)0.00;
            step.MinValue = (decimal)0.00;
            step.ValueUnit = "-";
            _context.ApplicationStep.Add(step);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return CreatedAtAction("GetApplciation", new { id = step.ApplicationStepId }, step);
        }

        [HttpPut("steps/{id}")]
        public async Task<IActionResult> PutStep([FromRoute] int id, [FromBody] ApplicationStep step)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != step.ApplicationStepId)
            {
                return BadRequest();
            }

            var existingStep = await _context.ApplicationStep.FirstOrDefaultAsync(a => a.ApplicationStepId == id);
            existingStep.StepTitle = step.StepTitle;
            existingStep.StepDescription = step.StepDescription;
            existingStep.SortOrder = step.SortOrder;
            // existingStep.IsRangeValue = step.IsRangeValue;
            // existingStep.MaxValue = step.MaxValue;
            // existingStep.MinValue = step.MinValue;
            // existingStep.ValueUnit = step.ValueUnit;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok(step);
        }

        [HttpPut("steps/step-details/{id}")]
        public async Task<IActionResult> PutStepDetail([FromRoute] int id, [FromBody] ApplicationStepDetail stepDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != stepDetail.ApplicationStepDetailId)
            {
                return BadRequest();
            }

            var existingStepDetail = await _context.ApplicationStepDetail.FirstOrDefaultAsync(a => a.ApplicationStepDetailId == id);
            existingStepDetail.StepDetailDescription = stepDetail.StepDetailDescription;
            existingStepDetail.SortOrder = stepDetail.SortOrder;
            existingStepDetail.StepDetailTitle = stepDetail.StepDetailTitle;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return Ok(stepDetail);
        }

        [HttpPost("steps/step-details")]
        public async Task<IActionResult> PostStepDetail([FromBody] ApplicationStepDetail stepDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            stepDetail.ApplicationStepDetailId = _context.ApplicationStepDetail.Max(l => l.ApplicationStepDetailId) + 1;
            _context.ApplicationStepDetail.Add(stepDetail);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return Ok(stepDetail);
        }

        [HttpDelete("steps/step-details/{id}")]
        public async Task<IActionResult> DeleteStepDetail([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var applicationStepDetail = await _context.ApplicationStepDetail.SingleOrDefaultAsync(m => m.ApplicationStepDetailId == id);
            if (applicationStepDetail == null)
            {
                return NotFound();
            }

            _context.ApplicationStepDetail.Remove(applicationStepDetail);
            await _context.SaveChangesAsync();

            return Ok(applicationStepDetail);
        }

        [HttpPost]
        [Route("step-details/{id}/Upload")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadAsync([FromRoute] int id, IFormFile file)
        {
            var exisintgApplicationStepDetail = await _context.ApplicationStepDetail.FirstOrDefaultAsync(m => m.ApplicationStepDetailId == id);
            if (exisintgApplicationStepDetail == null)
            {
                return BadRequest($"ApplicationStepDetailId {id} not found.");
            }

            var storageConnectionString = _config.GetConnectionString("AzureStorageConnectionString");

            if (!CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(CustomSolutionsContainerName);

            await container.CreateIfNotExistsAsync();

            // Delete existing files
            if (!string.IsNullOrEmpty(exisintgApplicationStepDetail.ThumbnailImagePath))
            {
                Uri uri = new Uri(exisintgApplicationStepDetail.ThumbnailImagePath);
                if (uri.IsFile)
                {
                    string fileName = Path.GetFileName(uri.LocalPath);
                    var existingBlob = container.GetBlockBlobReference(fileName);
                    await existingBlob.DeleteIfExistsAsync();
                }
                exisintgApplicationStepDetail.ThumbnailImagePath = null;
                await _context.SaveChangesAsync();

                if (file == null)
                {
                    return Ok();
                }
            }

            //MS: Don't rely on or trust the FileName property without validation. The FileName property should only be used for display purposes.
            var picBlob = container.GetBlockBlobReference(Guid.NewGuid().ToString() + "-" + file.FileName);
            await picBlob.UploadFromStreamAsync(file.OpenReadStream());

            exisintgApplicationStepDetail.ThumbnailImagePath = picBlob.Uri.AbsoluteUri;
            exisintgApplicationStepDetail.ThumbnailImageSize = file.Length.ToString();
            await _context.SaveChangesAsync();
            return Ok(exisintgApplicationStepDetail);
        }

        [HttpPost("step-details/{id}/tags")]
        public async Task<IActionResult> PostStepDetailTags([FromRoute] int id, [FromBody] List<ApplicationStepDetailTag> model)
        {
            var tags = _context.ApplicationStepDetailTag.Where(p => p.ApplicationStepDetailId == id);
            _context.ApplicationStepDetailTag.RemoveRange(tags);
            await _context.SaveChangesAsync();

            foreach (var tag in model)
            {
                tag.ApplicationStepDetailId = id;
                _context.ApplicationStepDetailTag.Add(tag);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
