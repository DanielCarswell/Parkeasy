using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Parkeasy.Models;

namespace Parkeasy
{
    public class Seed
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public Seed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void SeedUsers()
        {
            if (!_userManager.Users.Any())
            {
                int i = 0;

                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<ApplicationUser>>(userData);

                //string[] roles = {"Admin", "Manager", "Driver", "Booking Clerk",
                // "Invoice Clerk", "Valeting Staff", "Customer"};

                 var roles = new List<IdentityRole>
                 {
                     new IdentityRole{Name = "Admin"},
                     new IdentityRole{Name = "Manager"},
                     new IdentityRole{Name = "Driver"},
                     new IdentityRole{Name = "Booking Clerk"},
                     new IdentityRole{Name = "Invoice Clerk"},
                     new IdentityRole{Name = "Valeting Staff"},
                     new IdentityRole{Name = "Customer"}
                 };

                string[] userRoles = {"Admin", "Manager", "Booking Clerk", "Invoice Clerk",
                "Driver", "Valeting Staff", "Customer", "Customer", "Customer", "Customer"};

                string[] passwords = {"admin123", "manager123", "clerk123", "clerk123", "driver123"
                , "valeting123", "customer123", "customer123", "customer123", "customer123"};

                foreach (var role in roles)
                {
                    _roleManager.CreateAsync(role).Wait();
                }

                foreach (var user in users)
                {
                    _userManager.CreateAsync(user, passwords[i]).Wait();
                    _userManager.AddToRoleAsync(user, userRoles[i]).Wait();
                    i++;
                }
            }
        }
    }
}