using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceApi.Repositories;
using DinkToPdf.Contracts;
using EcommerceApi.Services;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Settings")]
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly EcommerceContext _context;
        
        public SettingsController(
            EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/Settings
        [HttpGet]
        public async Task<Settings> GetSettings()
        {
            return await _context.Settings.FirstOrDefaultAsync();
        }

        // POST: api/Settings
        [HttpPost]
        public async Task<IActionResult> PostSettings([FromBody] Settings settings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Settings.Any())
            {
                var currentSetting = await _context.Settings.FirstOrDefaultAsync();
                currentSetting.AdminEmail = settings.AdminEmail;
                currentSetting.FromEmail = settings.FromEmail;
                currentSetting.FromEmailPassword = settings.FromEmailPassword;
                currentSetting.ReportEmail = settings.ReportEmail;
                currentSetting.SmtpHost = settings.SmtpHost;
                currentSetting.SmtpPort = settings.SmtpPort;
                currentSetting.SmtpUseSsl = settings.SmtpUseSsl;
                currentSetting.BlockInSufficientStockOnOrder = settings.BlockInSufficientStockOnOrder;
                currentSetting.WarnInSufficientStockOnOrder = settings.WarnInSufficientStockOnOrder;
                currentSetting.AllowedIPAddresses = settings.AllowedIPAddresses;
                currentSetting.EnablePosIntegration = settings.EnablePosIntegration;
            }
            else
            {
                _context.Settings.Add(settings);
            }

            await _context.SaveChangesAsync();

            return Ok(settings);
        }
    }
}