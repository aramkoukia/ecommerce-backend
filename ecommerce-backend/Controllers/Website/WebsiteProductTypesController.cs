using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Website/ProductTypes")]
    public class WebsiteProductTypesController : Controller
    {
        private readonly EcommerceContext _context;

        public WebsiteProductTypesController(EcommerceContext context) => _context = context;

        // GET: api/ProductTypes
        [HttpGet]
        public IEnumerable<ProductType> GetProductTypes() => _context.ProductType;

    }
}