using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Website")]
    public class WebsiteProductTypesController : Controller
    {
        private readonly EcommerceContext _context;

        public WebsiteProductTypesController(EcommerceContext context) => _context = context;

        // GET: api/ProductTypes
        [HttpGet("ProductTypes")]
        public IEnumerable<ProductType> Get() => _context.ProductType;

    }
}