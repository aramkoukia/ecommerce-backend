﻿using System.Threading.Tasks;
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

namespace EcommerceApi.Controllers
{
    public class CustomerAuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger _logger;

        public CustomerAuthController(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtOptions> jwtOptions,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
            _logger = loggerFactory.CreateLogger<AuthController>();
        }

        [AllowAnonymous]
        [HttpPost("api/customer/auth/login")]
        [Produces("application/json")]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Ensure the username and password is valid.
            var user = await _userManager.FindByNameAsync(username);

            if (user != null && user.Disabled)
            {
                return BadRequest(new
                {
                    error = "", //OpenIdConnectConstants.Errors.InvalidGrant,
                    error_description = "Your user account is disabled."
                });
            }

            if (user != null && !user.IsCustomer)
            {
                return BadRequest(new
                {
                    error = "", //OpenIdConnectConstants.Errors.InvalidGrant,
                    error_description = "Invalid User Name."
                });
            }

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

            // Generate and issue a JWT token
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
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
                });
        }
    }
}