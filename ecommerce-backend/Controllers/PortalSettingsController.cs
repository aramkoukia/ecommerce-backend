using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/PortalSettings")]
    [AllowAnonymous]
    public class PortalSettingsController : Controller
    {
        private readonly EcommerceContext _context;
        
        public PortalSettingsController(
            EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/PortalSettings
        [HttpGet]
        public async Task<PortalSettings> GetPortalSettings() => await _context.PortalSettings.FirstOrDefaultAsync();
    }
}