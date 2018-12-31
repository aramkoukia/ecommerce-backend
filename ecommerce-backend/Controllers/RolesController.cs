using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace EcommerceApi.Controllers
{
    // [Authorize()]
    [Produces("application/json")]
    [Route("api/Roles")]
    public class RolesController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        

        public RolesController(
            EcommerceContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<IEnumerable<IdentityRole>> Get()
        {
            return await _roleManager.Roles.ToListAsync();
        }
    }
}