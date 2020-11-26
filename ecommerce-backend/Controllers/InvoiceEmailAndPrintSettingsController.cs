using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/InvoiceEmailAndPrintSettings")]
    [Authorize]
    public class InvoiceEmailAndPrintSettingsController : Controller
    {
        private readonly EcommerceContext _context;

        public InvoiceEmailAndPrintSettingsController(EcommerceContext context) =>
            _context = context;

        // GET: api/InvoiceEmailAndPrintSettings
        [HttpGet]
        public async Task<InvoiceEmailAndPrintSetting> GetSettings() =>
            await _context.InvoiceEmailAndPrintSetting.FirstOrDefaultAsync();

        // POST: api/InvoiceEmailAndPrintSettings
        [HttpPost]
        public async Task<IActionResult> PostSettings([FromBody] InvoiceEmailAndPrintSetting settings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Settings.Any())
            {
                var currentSetting = await _context.InvoiceEmailAndPrintSetting.FirstOrDefaultAsync();
                currentSetting.CCEmailAddress = settings.CCEmailAddress;
                currentSetting.EmailAttachmentFileName = settings.EmailAttachmentFileName;
                currentSetting.EmailBody = settings.EmailBody;
                currentSetting.EmailSubject = settings.EmailSubject;
                currentSetting.AdditionalChargesNote = settings.AdditionalChargesNote;
                currentSetting.Attention = settings.Attention;
                currentSetting.Footer1 = settings.Footer1;
                currentSetting.PayNote1 = settings.PayNote1;
                currentSetting.PayNote2 = settings.PayNote2;
                currentSetting.PayNote3 = settings.PayNote3;
                currentSetting.Signature = settings.Signature;
                currentSetting.StorePolicy = settings.StorePolicy;
            }
            else
            {
                settings.Id = 1;
                _context.InvoiceEmailAndPrintSetting.Add(settings);
            }

            await _context.SaveChangesAsync();

            return Ok(settings);
        }
    }
}