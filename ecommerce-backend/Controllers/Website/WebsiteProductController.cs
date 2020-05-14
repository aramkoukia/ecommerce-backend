using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Repositories;
using System.Threading.Tasks;
using EcommerceApi.ViewModel.Website;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Website")]
    public class WebsiteProductController : Controller
    {
        private readonly IProductTypeRepository _productTypeRepository;

        public WebsiteProductController(IProductTypeRepository productTypeRepository) =>
          _productTypeRepository = productTypeRepository;

        // GET: api/ProductTypes/{id}/Products
        [HttpGet("ProductTypes/{id}/Products")]
        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetAsync(string id) =>
          await _productTypeRepository.GetWebsiteProductsByProductType(id);

    }
}