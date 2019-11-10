using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using System;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/ClientPosSettings")]
    [Authorize]
    public class ClientPosSettingsController : Controller
    {
        private readonly EcommerceContext _context;

        public ClientPosSettingsController(EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/ClientPosSettings
        [HttpGet]
        public IEnumerable<ClientPosSettings> GetClientPosSettings()
        {
            return _context.ClientPosSettings;
        }

        // GET: api/ClientPosSetting/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClientPosSetting([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clientPosSetting = await _context.ClientPosSettings.SingleOrDefaultAsync(m => m.Id == id);

            if (clientPosSetting == null)
            {
                return NotFound();
            }

            return Ok(clientPosSetting);
        }

        // PUT: api/ClientPosSetting/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClientPosSetting([FromRoute] int id, [FromBody] ClientPosSettings clientPosSettings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != clientPosSettings.Id)
            {
                return BadRequest();
            }
            _context.Entry(clientPosSettings).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientPosSettingsExists(id))
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

        // POST: api/ClientPosSettings
        [HttpPost]
        public async Task<IActionResult> PostClientPosSetting([FromBody] ClientPosSettings clientPosSetting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.ClientPosSettings.Add(clientPosSetting);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ClientPosSettingsExists(clientPosSetting.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetClientPosSettings", new { id = clientPosSetting.Id }, clientPosSetting);
        }

        // DELETE: api/ClientPosSettings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientPosSettings([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clientPosSettings = await _context.ClientPosSettings.SingleOrDefaultAsync(m => m.Id == id);
            if (clientPosSettings == null)
            {
                return NotFound();
            }

            _context.ClientPosSettings.Remove(clientPosSettings);
            await _context.SaveChangesAsync();

            return Ok(clientPosSettings);
        }

        private bool ClientPosSettingsExists(int id)
        {
            return _context.ClientPosSettings.Any(e => e.Id == id);
        }
    }
}