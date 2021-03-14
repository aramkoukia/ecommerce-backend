using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using EcommerceApi.Repositories;
using EcommerceApi.ViewModel;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/custom-applications")]
    [Authorize]
    public class CustomApplicationsController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly ICustomApplicationRepository _customApplicationRepository;

        public CustomApplicationsController(
            EcommerceContext context,
            ICustomApplicationRepository customApplicationRepository
            )
        {
            _context = context;
            _customApplicationRepository = customApplicationRepository;
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
        public async Task<IActionResult> DeleteStep([FromBody] int id)
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
            // existingStep.IsRangeValue = step.IsRangeValue;
            // existingStep.MaxValue = step.MaxValue;
            // existingStep.MinValue = step.MinValue;
            existingStep.SortOrder = step.SortOrder;
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

            var existingStepDetail = await _context.ApplicationStepDetail.FirstOrDefaultAsync(a => a.ApplicationStepDetailId == detailid);
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
            stepDetail.ApplicationStepId = _context.ApplicationStepDetail.Max(l => l.ApplicationStepDetailId) + 1;
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
        public async Task<IActionResult> DeleteStepDetail([FromBody] int id)
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
    }
}
