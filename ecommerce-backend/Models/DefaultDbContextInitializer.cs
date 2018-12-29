using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EcommerceApi.Models
{
    public class DefaultDbContextInitializer : IDefaultDbContextInitializer
    {
        private readonly EcommerceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DefaultDbContextInitializer(EcommerceContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        public bool EnsureCreated()
        {
            return _context.Database.EnsureCreated();
        }

        public void Migrate()
        {
            _context.Database.Migrate();
        }

        public async Task Seed(IServiceProvider serviceProvider)
        {
            //adding custom roles
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            string[] roleNames = { "Admin", "Store Manager", "Sales Employee", "Inventory Employee" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                //creating the roles and seeding them to the database
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var email = "aramkoukia@gmail.com";
            var username = "aramkoukia";
            if (await _userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    GivenName = "Aram Koukia"
                };

                await _userManager.CreateAsync(user, "P2ssw0rd!");
            }

            email = "sales@gmail.com";
            username = "sales";
            if (await _userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    GivenName = "Sales User"
                };

                await _userManager.CreateAsync(user, "P2ssw0rd!");
                await UserManager.AddToRoleAsync(user, "Sales Employee");
            }

            email = "inventory@gmail.com";
            username = "inventory";
            if (await _userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    GivenName = "Inventory User"
                };

                await _userManager.CreateAsync(user, "P2ssw0rd!");
                await UserManager.AddToRoleAsync(user, "Inventory Employee");
            }

            email = "manager@gmail.com";
            username = "manager";
            if (await _userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    GivenName = "Manager User"
                };

                await _userManager.CreateAsync(user, "P2ssw0rd!");
                await UserManager.AddToRoleAsync(user, "Store Manager");
            }


            var role = await _roleManager.FindByNameAsync("Admin");
            if(role != null)
            {
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View New Order"));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Orders"));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Products"));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Inventory"));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Customers"));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Discounts"));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Locations"));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Taxes"));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Users"));
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Reports"));
            }

            //email = "info@lightsandparts.com";
            //username = "essishaney";
            //if (await _userManager.FindByEmailAsync(email) == null)
            //{
            //    var user = new ApplicationUser
            //    {
            //        UserName = username,
            //        Email = email,
            //        EmailConfirmed = true,
            //        GivenName = "Essi Shaney"
            //    };

            //    await _userManager.CreateAsync(user, "P2ssw0rd!");
            //    await UserManager.AddToRoleAsync(user, "Admin");
            //}

            _context.SaveChanges();
        }
    }

    internal class CustomClaimTypes
    {
        public static string Permission = "http://github.com/claims/permission";
    }

    public interface IDefaultDbContextInitializer
    {
        bool EnsureCreated();
        void Migrate();
        Task Seed(IServiceProvider serviceProvider);
    }
}
