using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceApi.ViewModel;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Products")]
    public class ProductsController : Controller
    {
        private readonly EcommerceContext _context;

        public ProductsController(EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<IEnumerable<object>> GetProduct()
        {


            return await _context
                .Product
                .AsNoTracking()
                .Select(p => new ProductViewModel
                {
                   ProductId = p.ProductId,
                   ProductCode = p.ProductCode,
                   ProductName = p.ProductName,
                   ChargeTaxes = p.ChargeTaxes,
                   AllowOutOfStockPurchase = p.AllowOutOfStockPurchase,
                   SalesPrice = p.SalesPrice,
                   PurchasePrice = p.PurchasePrice,
                   ModifiedDate = p.ModifiedDate,
                   ProductTypeId = p.ProductTypeId,
                   ProductTypeName = p.ProductType.ProductTypeName,
                   // VancouverBalance = p.ProductInventory.FirstOrDefault(l => l.LocationId == 1) == null ? 0 : p.ProductInventory.FirstOrDefault(l => l.LocationId == 1).Balance,
                   // AbbotsfordBalance = p.ProductInventory.FirstOrDefault(l => l.LocationId == 2) == null ? 0 : p.ProductInventory.FirstOrDefault(l => l.LocationId == 2).Balance,
                })
                .ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Product.AsNoTracking().SingleOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct([FromRoute] int id, [FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Product.SingleOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}