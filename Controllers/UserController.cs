using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeasy.Data;
using Parkeasy.Models;
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
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Overloaded Constructor for UserController.
        /// </summary>
        /// <param name="userManager">Instance of UserManager with ApplicationUser parameter.</param>
        /// <param name="context">Instance of ApplicationDbContext class.</param>
        public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Displays list of Users.
        /// </summary>
        /// <returns>Index View with users as model.</returns>
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

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            EditViewModel model = new EditViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                PostCode = user.PostCode
            };

            return View(model);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (model.Id == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    Id = model.Id,
                    Email = model.Email,
                    UserName = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    PostCode = model.PostCode
                };
                try
                {

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(string id)
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
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userData = await _context.Users
                .SingleOrDefaultAsync(u => u.Id == id);

            var user = new EditViewModel
            {
                Email = userData.Email,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                Address = userData.Address,
                PostCode = userData.PostCode
            };
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
