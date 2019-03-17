using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Controllers
{
    //[Authorize]
    [Produces("application/json")]
    [Route("api/CustomerStoreCredits")]
    public class CustomerStoreCreditsController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerStoreCreditsController(
            EcommerceContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/CustomerStoreCredits
        [HttpGet("{id}")]
        public IEnumerable<CustomerStoreCredit> GetCustomerStoreCredits([FromRoute] int id)
        {
            return _context.CustomerStoreCredit.AsNoTracking()
                .Where(c => c.CustomerId == id).ToList();
        }

        // POST: api/CustomerStoreCredits
        [HttpPost]
        public async Task<IActionResult> PostCustomerStoreCredits([FromBody] CustomerStoreCredit customerStoreCredit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(User);
            customerStoreCredit.CreatedByUserId = userId;
            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");
            customerStoreCredit.CreatedDate = date;
            _context.CustomerStoreCredit.Add(customerStoreCredit);

            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.CustomerId == customerStoreCredit.CustomerId);
            customer.StoreCredit += customerStoreCredit.Amount;

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomerStoreCredits", new { id = customerStoreCredit.CustomerId }, customerStoreCredit);
        }
    }
}