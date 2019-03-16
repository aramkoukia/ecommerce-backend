using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/CustomerStoreCredits")]
    public class CustomerStoreCreditsController : Controller
    {
        private readonly EcommerceContext _context;

        public CustomerStoreCreditsController(EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/CustomerStoreCredits
        [HttpGet]
        public IEnumerable<CustomerStoreCredit> GetCustomerStoreCredits(int customerId)
        {
            return _context.CustomerStoreCredit 
                .Where(c => c.CustomerId == customerId);
        }

        // POST: api/CustomerStoreCredits
        [HttpPost]
        public async Task<IActionResult> PostCustomerStoreCredits([FromBody] CustomerStoreCredit customerStoreCredit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.CustomerStoreCredit.Add(customerStoreCredit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomerStoreCredits", new { id = customerStoreCredit.CustomerId }, customerStoreCredit);
        }
    }
}