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

        // GET: api/website/products
        [HttpGet("Products")]
        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetAsync() =>
          await _productTypeRepository.GetWebsiteProducts();

        [HttpGet("ProductDetails")]
        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetDetailsAsync() =>
           await _productTypeRepository.GetWebsiteProductDetails();

        // GET: api/website/products
        [HttpGet("Products/New")]
        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetNewAsync() =>
          await _productTypeRepository.GetNewWebsiteProducts();

        [HttpGet("Products/Popular")]
        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetPopularAsync() =>
          await _productTypeRepository.GetPopularWebsiteProducts();

        // GET: api/ProductTypes/{id}/Products
        [HttpGet("ProductTypes/{id}/Products")]
        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetAsync(string id) =>
          await _productTypeRepository.GetWebsiteProductsByProductType(id);

        // GET: api/Website/products/slugs
        [HttpGet("Products/Slugs")]
        public async Task<IEnumerable<string>> GetSlugsAsync() =>
          await _productTypeRepository.GetWebsiteProductSlugs();

        // GET: api/Website/Products/{id}
        [HttpGet("Products/{id}/detail")]
        public async Task<WebsiteProductViewModel> GetProductAsync(string id) =>
          await _productTypeRepository.GetWebsiteProduct(id);
    }
}