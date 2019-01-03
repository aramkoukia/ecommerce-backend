﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System;

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

        public UsersController(
            EcommerceContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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
    }
}