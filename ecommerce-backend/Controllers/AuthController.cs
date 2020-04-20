using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using EcommerceApi.Models;
using EcommerceApi.Services;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace EcommerceApi.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger _logger;
        private readonly EcommerceContext _context;
        private readonly IHttpContextAccessor _accessor;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JwtOptions> jwtOptions,
            ILoggerFactory loggerFactory,
            EcommerceContext context,
            IHttpContextAccessor accessor)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtOptions = jwtOptions.Value;
            _accessor = accessor;
            _logger = loggerFactory.CreateLogger<AuthController>();
        }

        [AllowAnonymous]
        [HttpPost("api/auth/login")]
        [Produces("application/json")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var clientIp = _accessor.HttpContext.Connection.RemoteIpAddress.ToString().Replace("{", "").Replace("}","");
            if (!UserLocationIsAuthorized(clientIp))
            {
                return BadRequest(new
                {
                    error = "", //OpenIdConnectConstants.Errors.InvalidGrant,
                    error_description = $"You are not allowed to access the system from this location. IP: {clientIp}"
                });
            }
            // Ensure the username and password is valid.
            var user = await _userManager.FindByNameAsync(username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                return BadRequest(new
                {
                    error = "", //OpenIdConnectConstants.Errors.InvalidGrant,
                    error_description = "The username or password is invalid."
                });
            }

            // Ensure the email is confirmed.
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest(new
                {
                    error = "email_not_confirmed",
                    error_description = "You must have a confirmed email to log in."
                });
            }

            _logger.LogInformation($"User logged in (id: {user.Id})");

            var permissions = new List<string>();
            // Generate and issue a JWT token
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

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
            

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
              issuer: _jwtOptions.issuer,
              audience: _jwtOptions.issuer,
              claims: claims,
              expires: DateTime.Now.AddMinutes(540),
              signingCredentials: creds);

            return Ok(
                new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    permissions = permissions.Distinct(),
                    locations = await GetUserLocations(user.Id)
                });
        }

        private async Task<List<Location>> GetUserLocations(string userId)
        {
            var userLocations = await _context.UserLocation.Where(l => l.UserId == userId).ToListAsync();
            if (userLocations == null || !userLocations.Any())
            {
                return new List<Location>();
            }
            return _context.Location.Where(loc => userLocations.Select(l => l.LocationId).Contains(loc.LocationId)).ToList();
        }

        private bool UserLocationIsAuthorized(string clientIp)
        {
            var adminSafeList = _context.Settings.AsNoTracking().FirstOrDefault().AllowedIPAddresses;
            var ipList = adminSafeList?.Split(',');

            if (ipList == null || !ipList.Any())
            {
                return true;
            }

            return ipList.Contains(clientIp);
        }
    }
}