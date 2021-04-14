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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

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
        private readonly IHttpContextAccessor _accessor;

        public UsersController(
            EcommerceContext context,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager, 
            IHttpContextAccessor accessor)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _accessor = accessor;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IEnumerable<ApplicationUser>> Get()
        {
            var users = await _userManager.Users.Where(u => !u.Disabled).ToListAsync();
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
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.AuthCode != null && u.AuthCode.Equals(authCode, StringComparison.InvariantCultureIgnoreCase));
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

            var clientIp = _accessor.HttpContext.Connection.RemoteIpAddress.ToString().Replace("{", "").Replace("}", "");
            _context.LoginHistory.Add(
                new LoginHistory
                {
                    ClientIp = clientIp,
                    DisplayName = user.GivenName,
                    UserId = user.Id,
                    CreatedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time"),
                    LoginType = "Entered Pass Code"
                }
            );
            _context.SaveChanges();

            return Ok(
                new
                {
                    user,
                    permissions = permissions.Distinct(),
                });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] ApplicationUser user)
        {
            if (await _userManager.FindByEmailAsync(user.Email) == null 
                && await _userManager.FindByNameAsync(user.UserName) == null)
            {
                user.EmailConfirmed = true;
                user.IsCustomer = false;
                await _userManager.CreateAsync(user, user.PasswordHash);
                return Ok(user);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] ApplicationUser user)
        {
            if (await _userManager.FindByEmailAsync(user.Email) != null)
            {
                return BadRequest(
                new[] {
                    new
                    {
                        Code = "AlreadyExists",
                        Description = "Email already registered!"
                    }
                });
            }

            if (await _userManager.FindByNameAsync(user.UserName) != null)
            {
                return BadRequest(
                new[] {
                    new
                    {
                        Code = "AlreadyExists",
                        Description = "User Name already registered!"
                    }
                });
            }

            user.EmailConfirmed = false;
            user.IsCustomer = true;
            user.AuthCode = Guid.NewGuid().ToString();
            var result = await _userManager.CreateAsync(user, user.PasswordHash);
            if (result.Succeeded)
            {
                user = await _userManager.FindByEmailAsync(user.Email);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var portal = await _context.PortalSettings.FirstOrDefaultAsync();
                var url = $"{portal.PublicWebsiteUrl}confirm-email?userId={user.Id}&code={code}";
                _emailSender.SendEmailAsync(
                    user.Email,
                    $"{portal.PortalTitle} Registration - Confirm Email", 
                    $"Thanks for your Registration {user.GivenName}. \n" +
                    $"Use this link to activate your account: \n" +
                    $"{url}");
                return Ok(user);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(
                new[] {
                    new
                    {
                        Code = "UserNotFound",
                        Description = "Specified User Was Not Found!"
                    }
                });
            }

            var result = await _userManager.ConfirmEmailAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)));
            if (result.Succeeded)
            {
                return Ok(user);
            }
            else
            {
                return BadRequest(result.Errors);
            }
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

        [HttpPut("Info")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserViewModel updateUserViewModel)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(updateUserViewModel.UserName);

            user.GivenName = updateUserViewModel.GivenName;
            user.Email = updateUserViewModel.Email;
            await _userManager.UpdateAsync(user);

            _emailSender.SendEmailAsync(
                user.Email, "User Info Update", $"User Info Updated. <br> User Name: {updateUserViewModel.UserName}");

            return Ok(new { Succeeded = true });
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
                _emailSender.SendEmailAsync(
                    user.Email,
                    "Password Reset", $"Your password is reset. <br> Your new password is: {passwordResetInfo.NewPassword}");
            }
            return Ok(result);
        }

        [HttpPut("resetpasscode")]
        public async Task<IActionResult> ResetPasscode([FromBody] ResetPasscodeViewModel passcodeResetInfo)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(passcodeResetInfo.UserName);

            var otherUsersWithSamePasscode = _context.Users.Where(u => !u.UserName.Equals(passcodeResetInfo.UserName, StringComparison.InvariantCultureIgnoreCase) && u.AuthCode != null && u.AuthCode.Equals(passcodeResetInfo.NewPasscode, StringComparison.InvariantCultureIgnoreCase ));
            if (otherUsersWithSamePasscode != null && otherUsersWithSamePasscode.Any())
            {
                return Ok(new { Errors = new List<string>() { "This pass code is used by other users. please try a new passcode!" } });
            }

            var dbUser = _context.Users.FirstOrDefault(u => u.UserName.Equals(passcodeResetInfo.UserName, StringComparison.InvariantCultureIgnoreCase));
            dbUser.AuthCode = passcodeResetInfo.NewPasscode;
            await _context.SaveChangesAsync();

            _emailSender.SendEmailAsync(
                user.Email, "Passcode Reset", $"Your passcode is reset. <br> Your new passcode is: {passcodeResetInfo.NewPasscode}");

            return Ok( new { Succeeded = true });
        }

        [HttpDelete("{userName}")]
        public async Task<IActionResult> DeleteUser(string userName)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            user.Disabled = true;
            await _userManager.UpdateAsync(user);
            return Ok(new { Succeeded = true });
        }
    }
}