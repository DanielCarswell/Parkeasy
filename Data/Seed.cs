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
            if(!_userManager.Users.Any())
            {
                int i = 0;
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<ApplicationUser>>(userData);
                string[] passwords = {"admin123", "manager123", "staff123", "staff123", "driver123"
                , "clerk123", "clerk123", "customer123", "customer123", "customer123"};
                foreach(var user in users)
                {
                    _userManager.CreateAsync(user, passwords[i]).Wait();
                    i++;
                }
            }
        }
    }
}