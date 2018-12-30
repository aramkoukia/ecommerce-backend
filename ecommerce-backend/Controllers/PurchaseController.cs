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
using DinkToPdf.Contracts;
using DinkToPdf;
using EcommerceApi.Untilities;
using System.IO;
using EcommerceApi.Services;

namespace EcommerceApi.Controllers
{
    // [Authorize]
    [Produces("application/json")]
    [Route("api/Purchases")]
    public class PurchasesController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPurchaseRepository _PurchaseRepository;

        public PurchasesController(EcommerceContext context,
                                UserManager<ApplicationUser> userManager,
                                IPurchaseRepository PurchaseRepository)
        {
            _context = context;
            _userManager = userManager;
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
                    .ThenInclude(o =>o.Product)
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

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(User);
            Purchase.CreatedByUserId = userId;
            Purchase.CreatedDate = DateTime.UtcNow;
            Purchase.PurchaseDate = DateTime.UtcNow;

            _context.Purchase.Add(Purchase);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPurchase", new { id = Purchase.PurchaseId }, Purchase);
        }

        private bool PurchaseExists(int id)
        {
            return _context.Purchase.Any(e => e.PurchaseId == id);
        }
    }
}