using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace EcommerceApi.Controllers
{
    [Authorize()]
    [Produces("application/json")]
    [Route("api/ProductInventory")]
    public class ProductInventoryController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductInventoryController(
            EcommerceContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/ProductInventory
        [HttpGet]
        public IEnumerable<ProductInventoryHistory> GetProductInventoryHistory()
        {
            return _context.ProductInventoryHistory;
        }

        // GET: api/ProductInventory/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductInventoryHistory([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productInventoryHistory = await _context.ProductInventoryHistory.SingleOrDefaultAsync(m => m.ProductInventoryHistoryId == id);

            if (productInventoryHistory == null)
            {
                return NotFound();
            }

            return Ok(productInventoryHistory);
        }

        // PUT: api/ProductInventory/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductInventoryHistory([FromRoute] int id, [FromBody] ProductInventoryHistory productInventoryHistory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != productInventoryHistory.ProductInventoryHistoryId)
            {
                return BadRequest();
            }

            _context.Entry(productInventoryHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductInventoryHistoryExists(id))
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

        // POST: api/ProductInventory
        [HttpPost]
        public async Task<IActionResult> PostProductInventoryHistory([FromBody] ProductInventoryHistory productInventoryHistory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(User);
            productInventoryHistory.CreatedByUserId = userId;
            productInventoryHistory.ModifiedDate = DateTime.UtcNow;

            // Update Product Inventory
            var productInventory = await _context.ProductInventory.FirstOrDefaultAsync(m =>
               m.ProductId == productInventoryHistory.ProductId &&
               m.LocationId == productInventoryHistory.LocationId);

            if (productInventory != null)
            {
                productInventory.Balance = productInventoryHistory.Balance;
                productInventory.BinCode = productInventoryHistory.BinCode;
                productInventory.ModifiedDate = productInventoryHistory.ModifiedDate;
            }
            else {
                var newProductInventory = new ProductInventory
                {
                    Balance = productInventoryHistory.Balance,
                    BinCode = productInventoryHistory.BinCode,
                    LocationId = productInventoryHistory.LocationId,
                    ModifiedDate = productInventoryHistory.ModifiedDate,
                    ProductId = productInventoryHistory.ProductId
                };
                _context.ProductInventory.Add(newProductInventory);
            }

            _context.ProductInventoryHistory.Add(productInventoryHistory);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductInventoryHistory", new { id = productInventoryHistory.ProductInventoryHistoryId }, productInventoryHistory);
        }

        // DELETE: api/ProductInventory/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductInventoryHistory([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productInventoryHistory = await _context.ProductInventoryHistory.SingleOrDefaultAsync(m => m.ProductInventoryHistoryId == id);
            if (productInventoryHistory == null)
            {
                return NotFound();
            }

            _context.ProductInventoryHistory.Remove(productInventoryHistory);
            await _context.SaveChangesAsync();

            return Ok(productInventoryHistory);
        }

        private bool ProductInventoryHistoryExists(int id)
        {
            return _context.ProductInventoryHistory.Any(e => e.ProductInventoryHistoryId == id);
        }
    }
}