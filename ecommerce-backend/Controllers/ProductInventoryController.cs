using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EcommerceApi.Services;

namespace EcommerceApi.Controllers
{
    [Authorize()]
    [Produces("application/json")]
    [Route("api/ProductInventory")]
    public class ProductInventoryController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ProductInventoryController(
            EcommerceContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
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

            decimal currentBalance = 0;
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(User);
            productInventoryHistory.CreatedByUserId = userId;
            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");
            productInventoryHistory.ModifiedDate = date;

            // Update Product Inventory
            var productInventory = await _context.ProductInventory.FirstOrDefaultAsync(m =>
               m.ProductId == productInventoryHistory.ProductId &&
               m.LocationId == productInventoryHistory.LocationId);

            if (productInventory != null)
            {
                currentBalance = productInventory.Balance;
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

            // Calculate if the inventory is up or down
            productInventoryHistory.ChangedBalance = productInventoryHistory.Balance; // Storing the Balance after operation
            productInventoryHistory.Balance = productInventoryHistory.Balance - currentBalance;
            var operationType = "";
            operationType = productInventoryHistory.Balance > 0 ? "Stock Up - " : "Stock Down - ";
            productInventoryHistory.TransactionType = productInventoryHistory.Balance > 0 ? "Stock Up" : "Stock Down";
            productInventoryHistory.Notes = operationType + productInventoryHistory.Notes;

            if (productInventoryHistory.Balance != 0)
            {
                _context.ProductInventoryHistory.Add(productInventoryHistory);
            }

            await _context.SaveChangesAsync();

            if (productInventoryHistory.Balance != 0)
            {
                var product = await _context.Product.FirstOrDefaultAsync(p => p.ProductId == productInventoryHistory.ProductId);
                var location = await _context.Location.FirstOrDefaultAsync(p => p.LocationId == productInventoryHistory.LocationId);
                var subject = $"Inventory {productInventoryHistory.TransactionType} - {location.LocationName}";
                var message = $"Product: {product.ProductCode} - {product.ProductName}.\n";
                message += $"Inventory {productInventoryHistory.TransactionType}: {productInventoryHistory.Balance}.\n";
                message += $"Changed From: {currentBalance} To:{productInventory.Balance}. {productInventoryHistory.TransactionType}.\n";
                message += $"Date: {productInventoryHistory.ModifiedDate}.\n";
                message += $"Location: {location.LocationName}.\n";
                message += $"User: {userId}.\n";

                await _emailSender.SendEmailAsync(null, subject, null, message, null, null);
            }

            return CreatedAtAction("GetProductInventoryHistory", new { id = productInventoryHistory.ProductInventoryHistoryId }, productInventoryHistory);
        }

        [HttpPost("Transfer")]
        public async Task<IActionResult> PostTransferInventory([FromBody] TransferInventory transferInventory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(User);
            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");
            var fromProductInventoryHistory = new ProductInventoryHistory
            {
                CreatedByUserId = userId,
                ModifiedDate = date,
                ProductId = transferInventory.ProductId,
                Balance = transferInventory.TransferQuantity * -1,
                LocationId = transferInventory.FromLocationId,
                BinCode = "",
                Notes = "Transfer - " + transferInventory.TransferNotes,
                TransactionType = "Transfer",
            };
            var toProductInventoryHistory = new ProductInventoryHistory
            {
                CreatedByUserId = userId,
                ModifiedDate = date,
                ProductId = transferInventory.ProductId,
                Balance = transferInventory.TransferQuantity,
                LocationId = transferInventory.ToLocationId,
                BinCode = "",
                Notes = "Transfer - " + transferInventory.TransferNotes,
                TransactionType = "Transfer",
            };

            // Update Product Inventory
            var fromProductInventory = await _context.ProductInventory.FirstOrDefaultAsync(m =>
               m.ProductId == transferInventory.ProductId &&
               m.LocationId == transferInventory.FromLocationId);

            if (fromProductInventory != null)
            {
                fromProductInventory.Balance = fromProductInventory.Balance - transferInventory.TransferQuantity;
                fromProductInventory.ModifiedDate = fromProductInventoryHistory.ModifiedDate;
                fromProductInventoryHistory.ChangedBalance = fromProductInventory.Balance;
            }
            else
            {
                var newProductInventory = new ProductInventory
                {
                    Balance = -transferInventory.TransferQuantity,
                    BinCode = "",
                    LocationId = transferInventory.FromLocationId,
                    ModifiedDate = fromProductInventoryHistory.ModifiedDate,
                    ProductId = transferInventory.ProductId
                };
                fromProductInventoryHistory.ChangedBalance = newProductInventory.Balance;
                _context.ProductInventory.Add(newProductInventory);
            }

            var toProductInventory = await _context.ProductInventory.FirstOrDefaultAsync(m =>
               m.ProductId == transferInventory.ProductId &&
               m.LocationId == transferInventory.ToLocationId);

            if (toProductInventory != null)
            {
                toProductInventory.Balance = toProductInventory.Balance + transferInventory.TransferQuantity;
                toProductInventory.ModifiedDate = toProductInventoryHistory.ModifiedDate;
                toProductInventoryHistory.ChangedBalance = toProductInventory.Balance;
            }
            else
            {
                var newProductInventory = new ProductInventory
                {
                    Balance = transferInventory.TransferQuantity,
                    BinCode = "",
                    LocationId = transferInventory.ToLocationId,
                    ModifiedDate = toProductInventoryHistory.ModifiedDate,
                    ProductId = transferInventory.ProductId
                };
                toProductInventoryHistory.ChangedBalance = newProductInventory.Balance;
                _context.ProductInventory.Add(newProductInventory);
            }
            


            var product = await _context.Product.FirstOrDefaultAsync(p => p.ProductId == transferInventory.ProductId);
            var fromLocation = await _context.Location.FirstOrDefaultAsync(p => p.LocationId == transferInventory.FromLocationId);
            var toLocation = await _context.Location.FirstOrDefaultAsync(p => p.LocationId == transferInventory.ToLocationId);
            var subject = $"Inventory Transfer From: {fromLocation.LocationName} To: {toLocation.LocationName}";
            var message = $"Product: {product.ProductCode} - {product.ProductName}.\n";
            message += $"Inventory Transfer.\n";
            message += $"From: {fromLocation.LocationName} To: {toLocation.LocationName}.\n";
            message += $"Amount: {transferInventory.TransferQuantity}.\n";
            message += $"Date: {toProductInventoryHistory.ModifiedDate}.\n";
            message += $"User: {userId}.\n";
            await _emailSender.SendEmailAsync(null, subject, null, message, null, null);

            _context.ProductInventoryHistory.Add(fromProductInventoryHistory);
            _context.ProductInventoryHistory.Add(toProductInventoryHistory);

            await _context.SaveChangesAsync();

            return Ok(transferInventory.ProductId);
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