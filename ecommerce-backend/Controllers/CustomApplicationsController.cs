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

        // GET: api/custom-applications/5/steps
        [HttpGet("{id}/steps")]
        public async Task<IActionResult> GetSteps([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var stepDetails = await _context.ApplicationStepDetail.Where(m => m.ApplicationStepId == id).ToListAsync();
            return Ok(stepDetails);
        }

        // PUT: api/custom-applications/5/step
        [HttpPut("{id}/step")]
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
            existingStep.IsRangeValue = step.IsRangeValue;
            existingStep.MaxValue = step.MaxValue;
            existingStep.MinValue = step.MinValue;
            existingStep.SortOrder = step.SortOrder;
            existingStep.ValueUnit = step.ValueUnit;
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

        // POST: api/custom-applications/5/step
        [HttpPost("{id}/step")]
        public async Task<IActionResult> PostApplication([FromBody] ApplicationStep step)
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

        // PUT: api/custom-applications/5/step-detail
        [HttpPut("{id}/step-detail")]
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

    }
}