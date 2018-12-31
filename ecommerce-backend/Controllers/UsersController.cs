using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;

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
            var users  = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var locations = _context.UserLocation.AsNoTracking().Include(l => l.Location).Where(u => u.UserId == user.Id).Select(l=> l.Location.LocationName);
                user.Roles = string.Join(", ", await _userManager.GetRolesAsync(user));
                user.Locations = string.Join(", ", locations);
            }
            return users;
        }

        // GET: api/User/id/Roles
        [HttpGet("{id}/Roles")]
        public async Task<IEnumerable<string>> Get(string id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(currentUser);
            var user = await _userManager.FindByEmailAsync(userId);
            return await _userManager.GetRolesAsync(user);
        }
    }
}