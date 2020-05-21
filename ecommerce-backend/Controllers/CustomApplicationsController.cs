using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/custom-applications")]
    [Authorize]
    public class CustomApplicationsController : Controller
    {
        private readonly EcommerceContext _context;

        public CustomApplicationsController(EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/custom-applications
        [HttpGet]
        public IEnumerable<ApplicationStep> Get()
        {
            return _context.ApplicationStep;
        }

        // GET: api/custom-applications/5/steps
        [HttpGet("{id}/steps")]
        public async Task<IActionResult> GetSteps([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var stepDetails = await _context.ApplicationStepDetail.Where(m => m.StepId == id).ToListAsync();
            return Ok(stepDetails);
        }
    }
}