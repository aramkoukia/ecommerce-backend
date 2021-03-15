using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Repositories;
using System.Threading.Tasks;
using EcommerceApi.ViewModel;
using EcommerceApi.ViewModel.Website;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/website")]
    public class WebsiteCustomApplicationsController : Controller
    {
        private readonly ICustomApplicationRepository _customApplicationRepository;

        public WebsiteCustomApplicationsController(ICustomApplicationRepository customApplicationRepository) =>
          _customApplicationRepository = customApplicationRepository;

        // GET: api/website/custom-applications
        [HttpGet("custom-applications")]
        public async Task<IEnumerable<CustomApplicationViewModel>> Get()
          => await _customApplicationRepository.GetCustomApplicationSteps();

        // GET: api/website/custom-applications/result
        [HttpPost("custom-applications/result")]
        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetResult([FromBody] string[] selectedOptions)
          => await _customApplicationRepository.GetCustomApplicationResult(selectedOptions);
    }
}