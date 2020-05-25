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
    // [Authorize]
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
    }
}