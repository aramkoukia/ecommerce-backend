using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EcommerceApi.ViewModel;
using EcommerceApi.Repositories;
using EcommerceApi.Services;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Purchases")]
    public class PurchasesController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IPurchaseRepository _PurchaseRepository;

        public PurchasesController(EcommerceContext context,
                                UserManager<ApplicationUser> userManager,
                                IEmailSender emailSender,
                                IPurchaseRepository PurchaseRepository)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _PurchaseRepository = PurchaseRepository;
        }

        // GET: api/Purchases
        [HttpGet]
        public async Task<IEnumerable<PurchaseViewModel>> GetPurchase()
        {
            return await _PurchaseRepository.GetPurchases();
        }

        // GET: api/Purchases/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchase([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var Purchase = await _context.Purchase.AsNoTracking()
                .Include(o => o.PurchaseDetail)
                    .ThenInclude(o => o.Product)
                .Include(o => o.PurchaseDetail)
                    .ThenInclude(o => o.Location)
                .SingleOrDefaultAsync(m => m.PurchaseId == id);

            if (Purchase == null)
            {
                return NotFound();
            }

            return Ok(Purchase);
        }

        // POST: api/Purchases
        [HttpPost]
        public async Task<IActionResult> PostPurchase([FromBody] Purchase Purchase)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(User);
            Purchase.CreatedByUserId = userId;
            Purchase.CreatedDate = date;
            Purchase.PurchaseDate = date;

            foreach (var detail in Purchase.PurchaseDetail)
            {
                detail.CreatedByUserId = userId;
                detail.CreatedDate = date;
                detail.PoNumber = Purchase.PoNumber;
                detail.EstimatedDelivery = Purchase.DeliveryDate;
            }

            _context.Purchase.Add(Purchase);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPurchase", new { id = Purchase.PurchaseId }, Purchase);
        }

        [HttpPut("{id}/Status")]
        public async Task<IActionResult> PutPurchaseDetail([FromRoute] int id, [FromBody] UpdatePurchaseDetailStatus updatePurchaseDetailStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (updatePurchaseDetailStatus == null || string.IsNullOrEmpty(updatePurchaseDetailStatus.PurchaseStatus))
            {
                return BadRequest();
            }
            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");

            var purchaseDetail = await _context.PurchaseDetail.FirstOrDefaultAsync(m => m.PurchaseDetailId == id);

            if (purchaseDetail == null)
            {
                return BadRequest($"Invalid Purchase Detail Id : {id}");
            }
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(User);

            var newPurchaseDetail = new PurchaseDetail
            {
                Amount = updatePurchaseDetailStatus.Amount,
                ArrivedAtLocationId = updatePurchaseDetailStatus.ArrivedAtLocationId,
                ArrivedDate = updatePurchaseDetailStatus.ArrivedDate,
                PoNumber = updatePurchaseDetailStatus.PoNumber,
                CreatedByUserId = userId,
                CreatedDate = date,
                EstimatedDelivery = updatePurchaseDetailStatus.EstimatedDelivery,
                PaidDate = updatePurchaseDetailStatus.PaidDate,
                ProductId = purchaseDetail.ProductId,
                Status = updatePurchaseDetailStatus.PurchaseStatus,
                UnitPrice = updatePurchaseDetailStatus.UnitPrice,
                TotalPrice = updatePurchaseDetailStatus.TotalPrice,
                PurchaseId = purchaseDetail.PurchaseId
            };

            await _context.PurchaseDetail.AddAsync(newPurchaseDetail);

            // When purchase is marked as Arrived we should add them to inventory of the specified location
            if (updatePurchaseDetailStatus.PurchaseStatus == PurchaseStatus.Arrived.ToString() &&
                updatePurchaseDetailStatus.ArrivedAtLocationId.HasValue)
            {
                var done = await AddToInventory(updatePurchaseDetailStatus, purchaseDetail, date);
            }

            await _emailSender.SendAdminReportAsync("Purchase Status Changed", $"Purchase Status changed. \n Purchase Id: {id}. \n To: {updatePurchaseDetailStatus.PurchaseStatus.ToString()}");

            await _context.SaveChangesAsync();

            return Ok(newPurchaseDetail);
        }

        // DELETE: api/Purchases/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchase([FromRoute] int id)
        {
            var purchase = await _context.Purchase.FindAsync(id);
            if (purchase == null)
            {
                return NotFound();
            }

            var purchaseDetails = _context.PurchaseDetail.Where(p => p.PurchaseId == id);

            if (purchaseDetails != null)
            {
                foreach (var purchaseDetail in purchaseDetails)
                {
                    _context.PurchaseDetail.Remove(purchaseDetail);
                }
            }

            _context.Purchase.Remove(purchase);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Purchases/purchaseDetail/5
        [HttpDelete("purchasedetail/{id}")]
        public async Task<IActionResult> DeletePurchaseDetail([FromRoute] int id)
        {
            var purchaseDetail = await _context.PurchaseDetail.FindAsync(id);
            if (purchaseDetail == null)
            {
                return NotFound();
            }

            _context.PurchaseDetail.Remove(purchaseDetail);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<bool> AddToInventory(UpdatePurchaseDetailStatus updatePurchaseDetailStatus, PurchaseDetail purchaseDetail, DateTime date)
        {
            var productInventory = await _context.ProductInventory.FirstOrDefaultAsync(m =>
                                    m.ProductId == purchaseDetail.ProductId &&
                                    m.LocationId == updatePurchaseDetailStatus.ArrivedAtLocationId);

            decimal currentBalance = 0;
            if (productInventory != null)
            {
                currentBalance = productInventory.Balance;
                productInventory.Balance = productInventory.Balance + updatePurchaseDetailStatus.Amount;
                productInventory.ModifiedDate = date;
            }
            else
            {
                await _context.ProductInventory.AddAsync(
                    new ProductInventory
                    {
                        Balance = updatePurchaseDetailStatus.Amount,
                        BinCode = "",
                        LocationId = updatePurchaseDetailStatus.ArrivedAtLocationId.Value,
                        ModifiedDate = date,
                        ProductId = purchaseDetail.ProductId
                    });
            }

            var productInventoryHistory = new ProductInventoryHistory {
                ChangedBalance = currentBalance + updatePurchaseDetailStatus.Amount,
                Balance = updatePurchaseDetailStatus.Amount,
                Notes = $"Purchase Id: {purchaseDetail.PurchaseId} arrived",
                BinCode = "",
                LocationId = updatePurchaseDetailStatus.ArrivedAtLocationId.Value,
                CreatedByUserId = purchaseDetail.CreatedByUserId,
                ModifiedDate = date,
                ProductId = purchaseDetail.ProductId,
                TransactionType = "Purchase Arrived"
            };

            _context.ProductInventoryHistory.Add(productInventoryHistory);
            return true;
        }
    }
}