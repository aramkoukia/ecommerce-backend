using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System;
using EcommerceApi.Services;

namespace EcommerceApi.Controllers
{
    [Authorize()]
    [Produces("application/json")]
    [Route("api/Users")]
    public class UsersController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;

        public UsersController(
            EcommerceContext context,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IEnumerable<ApplicationUser>> Get()
        {
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var locations = _context.UserLocation.AsNoTracking().Include(l => l.Location).Where(u => u.UserId == user.Id).Select(l => l.Location.LocationName);
                user.Roles = string.Join(", ", await _userManager.GetRolesAsync(user));
                user.Locations = string.Join(", ", locations);
            }
            return users;
        }

        // GET: api/Users/{authCode}
        [HttpGet("{authCode}")]
        public async Task<IActionResult> Get(string authCode)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.AuthCode.Equals(authCode, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                NotFound();
            }

            var permissions = new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    if (roleClaims != null && roleClaims.Any())
                    {
                        permissions.AddRange(roleClaims.Select(c => c.Value));
                    }
                }
            }

            return Ok(
                new
                {
                    user,
                    permissions = permissions.Distinct(),
                });
        }

        // GET: api/Users/id/Roles
        [HttpGet("{email}/Roles")]
        public async Task<IEnumerable<string>> GetUserRoles(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return await _userManager.GetRolesAsync(user);
        }

        // GET: api/Users/id/Locations
        [HttpGet("{email}/Locations")]
        public async Task<IEnumerable<UserLocation>> GetUserLocations(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return await _context.UserLocation.Where(l => l.UserId == user.Id).ToListAsync();
        }

        // PUT: api/Users/Permissions
        [HttpPut("Permissions")]
        public async Task<IActionResult> PutUserPermissions([FromBody] UpdateUserPermissionViewModel userInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Udate user roles
            var user = await _userManager.FindByEmailAsync(userInfo.Email);
            var allRoles = _roleManager.Roles.ToList();

            foreach (var role in allRoles)
            {
                if (userInfo.RoleIds.Contains(role.Id))
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
            }

            // Update location assignment
            var userLocations = _context.UserLocation.Where(m => m.UserId == user.Id);
            _context.UserLocation.RemoveRange(userLocations);

            var userLocationList = userInfo.LocationIds.Select(l =>
                new UserLocation {
                    LocationId = l,
                    UserId = user.Id
                });

            _context.UserLocation.AddRange(userLocationList);

            await _context.SaveChangesAsync();

            return Ok(userInfo);
        }

        [HttpPut("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel passwordResetInfo)
        {
            ApplicationUser user = null;
            if(!string.IsNullOrEmpty(passwordResetInfo.Email))
                user = await _userManager.FindByEmailAsync(passwordResetInfo.Email);
            else 
                user = await _userManager.FindByNameAsync(passwordResetInfo.UserName);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, passwordResetInfo.NewPassword);
            if (result.Succeeded)
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Password Reset", $"Your password is reset. <br> Your new password is: {passwordResetInfo.NewPassword}");
            }
            return Ok(result);
        }

        [HttpPut("resetpasscode")]
        public async Task<IActionResult> ResetPasscode([FromBody] ResetPasscodeViewModel passcodeResetInfo)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(passcodeResetInfo.UserName);

            var otherUsersWithSamePasscode = _context.Users.Where(u => !u.UserName.Equals(passcodeResetInfo.UserName, StringComparison.InvariantCultureIgnoreCase) && u.AuthCode.Equals(passcodeResetInfo.NewPasscode, StringComparison.InvariantCultureIgnoreCase ));
            if (otherUsersWithSamePasscode != null && otherUsersWithSamePasscode.Any())
            {
                return BadRequest(new { Errors = new List<string>() { "This pass code is used by other users. please try a new passcode!" } });
            }

            var dbUser = _context.Users.FirstOrDefault(u => u.UserName.Equals(passcodeResetInfo.UserName, StringComparison.InvariantCultureIgnoreCase));
            dbUser.AuthCode = passcodeResetInfo.NewPasscode;
            await _context.SaveChangesAsync();

            await _emailSender.SendEmailAsync(
                user.Email, "Passcode Reset", $"Your passcode is reset. <br> Your new passcode is: {passcodeResetInfo.NewPasscode}");

            return Ok( new { Succeeded = true });
        }
    }
}