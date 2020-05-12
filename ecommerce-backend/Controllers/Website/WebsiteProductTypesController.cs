using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Repositories;
using EcommerceApi.ViewModel.Website;
using System.Threading.Tasks;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Website")]
    public class WebsiteProductTypesController : Controller
    {
        private readonly IProductTypeRepository _productTypeRepository;

        public WebsiteProductTypesController(IProductTypeRepository productTypeRepository) =>
          _productTypeRepository = productTypeRepository;

        // GET: api/ProductTypes
        [HttpGet("ProductTypes")]
        public async Task<IEnumerable<WebsiteProductTypeViewModel>> GetAsync() =>
          await _productTypeRepository.GetWebsiteProductTypes();

    }
}