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
using System.Linq;

namespace EcommerceApi.Controllers
{
    public class CustomerAuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EcommerceContext _context;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger _logger;

        public CustomerAuthController(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtOptions> jwtOptions,
            EcommerceContext context,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
            _context = context;
            _logger = loggerFactory.CreateLogger<AuthController>();
        }

        [AllowAnonymous]
        [HttpPost("api/customer/auth/login")]
        [Produces("application/json")]
        public async Task<IActionResult> Login([FromBody] LoginInfo user)
        {
            // Ensure the username and password is valid.
            var foundUser = await _userManager.FindByNameAsync(user.username);

            if (foundUser == null)
            {
                return BadRequest(new
                {
                    error = "", //OpenIdConnectConstants.Errors.InvalidGrant,
                    error_description = "Invalid User Name."
                });
            }

            if (foundUser.Disabled)
            {
                return BadRequest(new
                {
                    error = "", //OpenIdConnectConstants.Errors.InvalidGrant,
                    error_description = "Your user account is disabled."
                });
            }

            if (!foundUser.IsCustomer)
            {
                return BadRequest(new
                {
                    error = "", //OpenIdConnectConstants.Errors.InvalidGrant,
                    error_description = "User Name is invalid."
                });
            }

            // Ensure the email is confirmed.
            if (!await _userManager.IsEmailConfirmedAsync(foundUser))
            {
                return BadRequest(new
                {
                    error = "email_not_confirmed",
                    error_description = "You must confirm your email to log in."
                });
            }

            if (!await _userManager.CheckPasswordAsync(foundUser, user.password))
            {
                return BadRequest(new
                {
                    error = "", //OpenIdConnectConstants.Errors.InvalidGrant,
                    error_description = $"Username or password is invalid."
                });
            }

            _logger.LogInformation($"User logged in (id: {foundUser.Id})");

            // Generate and issue a JWT token
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, foundUser.Email),
                new Claim(JwtRegisteredClaimNames.Sub, foundUser.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // var permissions = new List<string>();
            //var roles = await _userManager.GetRolesAsync(user);
            //foreach (var roleName in roles)
            //{
            //    var role = await _roleManager.FindByNameAsync(roleName);
            //    if (role != null)
            //    {
            //        var roleClaims = await _roleManager.GetClaimsAsync(role);
            //        if (roleClaims != null && roleClaims.Any())
            //        {
            //            permissions.AddRange(roleClaims.Select(c => c.Value));
            //        }
            //    }
            //}

            var completeProfile = _context.CustomerUsers.Any(c => c.UserId == foundUser.Id);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
              issuer: _jwtOptions.issuer,
              audience: _jwtOptions.issuer,
              claims: claims,
              expires: DateTime.Now.AddMinutes(540),
              signingCredentials: creds);

            // TODO: create a separate customer login history if needed
            //_context.LoginHistory.Add(
            //    new LoginHistory
            //    {
            //        ClientIp = clientIp,
            //        DisplayName = user.GivenName,
            //        UserId = user.Id,
            //        CreatedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time"),
            //        LoginType = "User Login"
            //    }
            //);
            // _context.SaveChanges();

            return Ok(
                new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    completeProfile
                });
        }
    }
}