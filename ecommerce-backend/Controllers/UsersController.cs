using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace EcommerceApi.Controllers
{
    [Authorize()]
    [Produces("application/json")]
    [Route("api/Users")]
    public class UsersController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(
            EcommerceContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IEnumerable<ApplicationUser>> Get()
        {
            return await _userManager.Users.ToListAsync();
        }

        // GET: api/User/id/Roles
        [HttpGet("{id}/Roles")]
        public async Task<IEnumerable<string>> Get(string id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.GetUserAsync(User);
            return await _userManager.GetRolesAsync(user);
        }
    }
}