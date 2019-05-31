using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Parkeasy.Data;
using Parkeasy.Models;
using Parkeasy.Services;
using Parkeasy.Extensions;
using Parkeasy.Models.AccountViewModels;
using Microsoft.AspNetCore.Identity;

namespace Parkeasy.Controllers
{
    /// <summary>
    /// UserController class.
    /// </summary>
    public class UserController : Controller
    {
        /// <summary>
        /// UserManager and ApplicationDbContext Global Variables.
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Overloaded Constructor for UserController.
        /// </summary>
        /// <param name="userManager">Instance of UserManager with ApplicationUser parameter.</param>
        /// <param name="context">Instance of ApplicationDbContext class.</param>
        public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
        }

        /// <summary>
        /// Displays list of Users.
        /// </summary>
        /// <returns>Index View with users as model.</returns>
        [Authorize(Roles = "Admin,Manager,Booking Clerk,Invoice Clerk")]
        public async Task<IActionResult> Index()
        {
            //Gets all users from database using _context parameter.
            var users = new List<UserViewModel>();
            await users.GetUsers(_context);

            //Passes users variable to User/Index as model.
            return View(users);
        }

        /// <summary>
        /// Displays list of Customers.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Booking Clerk,Invoice Clerk")]
        public async Task<IActionResult> CustomerIndex()
        {
            //Gets all customers from database using _context parameter.
            var users = new List<UserViewModel>();
            await users.GetCustomers(_context);

            //Passes users variable to User/Index as model.
            return View(users);
        }

        /// <summary>
        /// Create ViewData for Job Name then loads CreateStaff View.
        /// </summary>
        /// <returns>Create View</returns>
        [ActionName("Create")]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateStaff()
        {
            //Creates new roles variable as List of IdentityRoles.
            List<IdentityRole> roles = new List<IdentityRole>();
            
            //Sets roles equal to all Roles that have a Name NOT EQUAL to Customer.
            roles = _context.Roles.Where(r => !r.Name.Equals("Customer")).ToList();
            
            //Creates ViewData "Name" for displaying Job Titles in View.
            ViewData["Name"] = new SelectList(roles, "Name", "Name");

            //Returns the CreateStaff View.
            return View();
        }

        /// <summary>
        /// Creates a new ApplicationUser using data in model variable and attempts to add
        /// that ApplicationUser to the database with the Job Title as role.
        /// </summary>
        /// <param name="model">Instance of StaffRegisterViewModel class.</param>
        /// <returns>Redirect to Index View or Create if adding user to database fails.</returns>
        [HttpPost]
        [ActionName("Create")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(StaffRegisterViewModel model)
        {
            //Creating new ApplicationUser Instance using data from model variable.
            ApplicationUser newUser = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                JobTitle = model.Name,
                CurrentQualification = model.Qualification,
                EmergencyContact = model.EmergencyContact
            };

            //Attempts to add newUser to database through the UserManager and stores result in result variable.
            var result = await _userManager.CreateAsync(newUser, model.Password);

            //Checks if adding newUser to database succeeded.
            if(result.Succeeded)
            {
                //Adds newUser to the Role/Job Title selected in CreateStaff view.
                _userManager.AddToRoleAsync(newUser, model.Name).Wait();

                //Redirects to Index action.
                return RedirectToAction(nameof(Index));
            }

            //Creates new roles variable as List of IdentityRoles.
            List<IdentityRole> roles = new List<IdentityRole>();

            //Sets roles equal to all Roles that have a Name NOT EQUAL to Customer.
            roles = await _context.Roles.Where(r => !r.Name.Equals("Customer")).ToListAsync();

            //Creates ViewData "Name" for displaying Job Titles in View.
            ViewData["Name"] = new SelectList(roles, "Name", "Name");

            //Returns the CreateStaff view with model of data entered before.
            return View(model);
        }

        /// <summary>
        /// Gets user and role then displays in a view with List of roles.
        /// </summary>
        /// <param name="id">String Variable.</param>
        /// <returns>ChangeRole View.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(string id)
        {
            //Creates new roles variable as List of IdentityRoles.
            List<IdentityRole> roles = new List<IdentityRole>();

            //Create new Instance of ChangeroleViewModel: userToChange.
            ChangeRoleViewModel userToChange = new ChangeRoleViewModel();

            //Get user and role.
            userToChange = Parkeasy.Extensions.IdentityExtensions.GetUserAndRole(_context, id);
        
            //Sets roles equal to all Roles that have a Name NOT EQUAL to Customer.
            roles = await _context.Roles.Where(r => !r.Name.Equals("Customer")).ToListAsync();

            //Creates ViewData "Name" for displaying Job Titles in View.
            ViewData["Name"] = new SelectList(roles, "Name", "Name");

            //Return View with userToChange as model.
            return View(userToChange);
        }

        /// <summary>
        /// Attempts to change users role.
        /// </summary>
        /// <param name="model">Instance of ChangeRoleViewModel class.</param>
        /// <returns>Redirect to Index Action or displays ChangeRole view with a model.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(ChangeRoleViewModel model)
        {
            //Creating new Instance of ApplicationUser class.
            ApplicationUser user = new ApplicationUser();

            //Gets user from database using Id.
            user = await _context.Users.Where(u => u.Id.Equals(model.Id)).FirstOrDefaultAsync();

            //Attempts to remove user from role and sets result variable to result.
            var result = await _userManager.RemoveFromRoleAsync(user, model.Role);

            //If result succeeded.
            if(result.Succeeded)
            {
                //Attempts to add user to a new role and sets result variable to result.
            result = await _userManager.AddToRoleAsync(user, model.Name);

            //If result succeeded.
            if(result.Succeeded)
            {
                //Returns redirect to action Index.
                return RedirectToAction(nameof(Index));
            }

            //Attempts to add user to a old role and sets result variable to result.
            await _userManager.AddToRoleAsync(user, model.Role);
            }

            //Returns ChangeRole view and passes in a model.
            return View(model);
        }

        // GET: User/Details/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .SingleOrDefaultAsync(m => m.Id == id);
            
            RegisterViewModel model = new RegisterViewModel
            {
                PostCode = user.PostCode,
                Address = user.Address,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Telephone = user.Telephone
                
            };
            if (user == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> StaffDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .SingleOrDefaultAsync(m => m.Id == id);
            
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userData = await _context.Users
                .SingleOrDefaultAsync(u => u.Id == id);

            if (userData == null)
            {
                return NotFound();
            }

            return View(userData);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Customer")]
        public IActionResult SendEnquiry()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> SendEnquiry(Invoice invoice)
        {
            await _emailSender.SendEnquiryAsync("danielcarswelldrive@gmail.com", invoice.InvoiceType, invoice.InvoiceBody);

            return RedirectToAction(nameof(HomeController.Home), "Home");
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
