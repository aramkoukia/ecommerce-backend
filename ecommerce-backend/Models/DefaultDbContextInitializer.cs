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

        public Task Seed(IServiceProvider serviceProvider)
        {
            //adding custom roles
            //var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            //var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            //string[] roleNames = { "Admin", "Store Manager", "Sales Employee", "Inventory Employee", "Supply Employee" };
            //IdentityResult roleResult;

            //foreach (var roleName in roleNames)
            //{
            //    //creating the roles and seeding them to the database
            //    var roleExist = await RoleManager.RoleExistsAsync(roleName);
            //    if (!roleExist)
            //    {
            //        roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
            //    }
            //}

            //var email = "aramkoukia@gmail.com";
            //var aram = await _userManager.FindByEmailAsync(email);
            //await UserManager.AddToRoleAsync(aram, "Admin");

            //email = "aramkoukia@gmail.com";
            //var username = "aramkoukia";
            //if (await _userManager.FindByEmailAsync(email) == null)
            //{
            //    var user = new ApplicationUser
            //    {
            //        UserName = username,
            //        Email = email,
            //        EmailConfirmed = true,
            //        GivenName = "Aram Koukia"
            //    };

            //    await _userManager.CreateAsync(user, "P2ssw0rd!");
            //    await UserManager.AddToRoleAsync(user, "Admin");
            //}

            //var role = await _roleManager.FindByNameAsync("Admin");
            //if(role != null)
            //{
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View New Order"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Orders"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View New Purchase"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Purchases"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Products"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Inventory"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Customers"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Discounts"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Locations"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Taxes"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Users"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Roles"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Reports"));
            //}

            //role = await _roleManager.FindByNameAsync("Store Manager");
            //if (role != null)
            //{
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View New Order"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Orders"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Products"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Inventory"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Customers"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Reports"));
            //}

            //role = await _roleManager.FindByNameAsync("Sales Employee");
            //if (role != null)
            //{
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View New Order"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Orders"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Products"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Customers"));
            //}

            //role = await _roleManager.FindByNameAsync("Inventory Employee");
            //if (role != null)
            //{
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Products"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Inventory"));
            //}

            //role = await _roleManager.FindByNameAsync("Supply Employee");
            //if (role != null)
            //{
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View New Purchase"));
            //    await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, "View Purchases"));
            //}

            //_context.SaveChanges();
            return Task.FromResult(true);
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
