using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Parkeasy.Models;

namespace Parkeasy.Data
{
    /// <summary>
    /// Class for seeding database data.
    /// </summary>
    public class Seed
    {
        /// <summary>
        /// UserManager, RoleManager global variables for seeding users, and ApplicationDbContext for 
        /// seeding other database tables.
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for Seed class, also populated _userManager and _roleManager global variables.
        /// </summary>
        /// <param name="userManager">Instance of UserManager with ApplicationUser parameter.</param>
        /// <param name="roleManager">Instance of RoleManager with IdentityRole parameter.</param>
        public Seed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            //Setting global variables.
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        /// <summary>
        /// Method for Seeding users with their intended Roles.
        /// </summary>
        public void SeedUsers()
        {
            // Checks if Users have already been seeded, if NOT runs code inside.
            if (!_userManager.Users.Any())
            {
                //Variable for handling position of data in arrays in context of Iterations.
                int i = 0;

                //Getting data from JSON file and deserializing.
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<ApplicationUser>>(userData);

                //Initialising List of Roles with parameter IdentityRole.
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

                 //Creating array of Roles for the seeded users.
                string[] userRoles = {"Admin", "Manager", "Booking Clerk", "Invoice Clerk",
                "Driver", "Valeting Staff", "Customer", "Customer", "Customer", "Customer"};

                //Creating array of Passwords for seeded users.
                string[] passwords = {"admin123", "manager123", "clerk123", "clerk123", "driver123"
                , "valeting123", "customer123", "customer123", "customer123", "customer123"};

                //Loops for every role in the roles List.
                foreach (var role in roles)
                {
                    //Adds role to RoleManager.
                    _roleManager.CreateAsync(role).Wait();
                }

                //Loops for every user in users array recieved from JSON file.
                foreach (var user in users)
                {
                    //Creates users using UserManager with parameter user details and password asynchronously.
                    _userManager.CreateAsync(user, passwords[i]).Wait();

                    //Adds newly created user to a role with parameter user details and userRole asynchronously.
                    _userManager.AddToRoleAsync(user, userRoles[i]).Wait();
                    
                    //Add 1 to i for next Iteration.
                    i++;
                }
            }
        }

        /// <summary>
        /// Seeds the Slot Table with 150 Blank Slots with Available status's.
        /// </summary>
        public void SeedSlotData()
        {
            //Checks if any Slots exist already in database.
            //Runs if statement if NO slot data exists.
            if(!_context.Slots.Any())
            {
                //Creates new instance of Slot class with Status set to "Available".
                Slot slot = new Slot
                {
                    Status = "Available"
                };

                //Loops 150 times adding Slots to Database as Car Park has 150 slots.
                for(int i = 1; i <= 150; i++)
                {
                    slot.Id = i;
                    _context.Slots.Add(slot);
                    _context.SaveChanges();
                }
            }
        }
    }
}