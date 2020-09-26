using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/CustomerStatementSettings")]
    [Authorize]
    public class CustomerStatementSettingsController : Controller
    {
        private readonly EcommerceContext _context;

        public CustomerStatementSettingsController(EcommerceContext context) =>
            _context = context;

        // GET: api/CustomerStatementSettings
        [HttpGet]
        public async Task<CustomerStatementSetting> GetSettings() =>
            await _context.CustomerStatementSetting.FirstOrDefaultAsync();

        // POST: api/CustomerStatementSettings
        [HttpPost]
        public async Task<IActionResult> PostSettings([FromBody] CustomerStatementSetting settings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Settings.Any())
            {
                var currentSetting = await _context.CustomerStatementSetting.FirstOrDefaultAsync();
                currentSetting.CCEmailAddress = settings.CCEmailAddress;
                currentSetting.EmailAttachmentFileName = settings.EmailAttachmentFileName;
                currentSetting.EmailBody = settings.EmailBody;
                currentSetting.EmailSubject = settings.EmailSubject;            }
            else
            {
                settings.Id = 1;
                _context.CustomerStatementSetting.Add(settings);
            }

            await _context.SaveChangesAsync();

            return Ok(settings);
        }
    }
}