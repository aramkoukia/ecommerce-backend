using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Linq;

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

        [HttpGet("Permissions")]
        public async Task<IEnumerable<string>> GetPermissions()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var claims = new List<Claim>();
            foreach (var role in roles)
            {
                claims.AddRange(await _roleManager.GetClaimsAsync(role));
            }

            return claims.Select(m => m.Value).Distinct();
        }

        [HttpGet("{roleName}/Permissions")]
        public async Task<IEnumerable<string>> GetRolePermissions(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            var claims = await _roleManager.GetClaimsAsync(role);
            return claims.Select(m => m.Value).Distinct();
        }

        // PUT: api/Users/Permissions
        [HttpPut("Permissions")]
        public async Task<IActionResult> PutRolePermissions([FromBody] UpdateRolePermissionViewModel roleInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Udate user roles
            var roles = await _roleManager.Roles.ToListAsync();
            var selectedRole = await _roleManager.FindByNameAsync(roleInfo.RoleName);
            var claims = new List<Claim>();
            foreach (var role in roles)
            {
                claims.AddRange(await _roleManager.GetClaimsAsync(role));
            }
            var claimValues = claims.Select(c => c.Value).Distinct();

            foreach (var claim in claimValues)
            {
                if (!roleInfo.Claims.Contains(claim))
                {
                    await _roleManager.RemoveClaimAsync(selectedRole, claims.FirstOrDefault(m=> m.Value == claim));
                }
                else
                {
                    await _roleManager.AddClaimAsync(selectedRole, claims.FirstOrDefault(m => m.Value == claim));
                }
            }
            return Ok(roleInfo);
        }
    }
}