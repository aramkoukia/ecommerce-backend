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
    [AllowAnonymous]
    public class PortalSettingsController : Controller
    {
        private readonly EcommerceContext _context;
        
        public PortalSettingsController(
            EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/Settings
        [HttpGet]
        public async Task<Settings> GetPortalSettings()
        {
            return await _context.PortalSettings.FirstOrDefaultAsync();
        }
    }
}