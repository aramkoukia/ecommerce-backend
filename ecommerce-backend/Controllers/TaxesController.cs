using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Taxes")]
    public class TaxesController : Controller
    {
        private readonly EcommerceContext _context;

        public TaxesController(EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/Taxes
        [HttpGet]
        public IEnumerable<Tax> GetTax(string country, string province)
        {
            return _context.Tax
                .Where(t => 
                (string.IsNullOrEmpty(country) || t.Country.Equals(country,System.StringComparison.InvariantCultureIgnoreCase)) &&
                (string.IsNullOrEmpty(province) || t.Province.Equals(province, System.StringComparison.InvariantCultureIgnoreCase)));
        }

        // GET: api/Taxes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTax([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tax = await _context.Tax.SingleOrDefaultAsync(m => m.TaxId == id);

            if (tax == null)
            {
                return NotFound();
            }

            return Ok(tax);
        }

        // PUT: api/Taxes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTax([FromRoute] int id, [FromBody] Tax tax)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tax.TaxId)
            {
                return BadRequest();
            }

            _context.Entry(tax).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxExists(id))
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

        // POST: api/Taxes
        [HttpPost]
        public async Task<IActionResult> PostTax([FromBody] Tax tax)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Tax.Add(tax);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTax", new { id = tax.TaxId }, tax);
        }

        // DELETE: api/Taxes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTax([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tax = await _context.Tax.SingleOrDefaultAsync(m => m.TaxId == id);
            if (tax == null)
            {
                return NotFound();
            }

            _context.Tax.Remove(tax);
            await _context.SaveChangesAsync();

            return Ok(tax);
        }

        private bool TaxExists(int id)
        {
            return _context.Tax.Any(e => e.TaxId == id);
        }
    }
}