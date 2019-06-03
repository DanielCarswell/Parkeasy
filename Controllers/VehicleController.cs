using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Parkeasy.Data;
using Parkeasy.Models;
using Parkeasy.Models.BookingViewModels;
using Microsoft.AspNetCore.Http;

namespace Parkeasy.Controllers
{
    /// <summary>
    /// Controller for vehicle class related functionality.
    /// </summary>
    public class VehicleController : Controller
    {
        /// <summary>
        /// Global Variables.
        /// </summary>
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Overloaded Constructor for initialising globals.
        /// </summary>
        /// <param name="context">ApplicationDbContext Class Instance.</param>
        /// <param name="userManager">UserManager Class Insance with ApplicationUser param.</param>
        public VehicleController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Displays all vehicle details.
        /// </summary>
        /// <returns>Index View</returns>
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Vehicles.Include(v => v.Booking);
            return View(await applicationDbContext.ToListAsync());
        }


        /// <summary>
        /// Method for adding Vehicle to a booking.
        /// </summary>
        /// <param name="booking">Booking Class Instance</param>
        /// <returns>Create View</returns>
        public IActionResult Create(Booking booking)
        {
            //Initialising vehicle class for model.
            Vehicle vehicle = new Vehicle
            {
                Booking = booking,
                Id = booking.Id,
            };

            //Passing model to view.
            return View(vehicle);
        }

        /// <summary>
        /// Adds a new Vehicle class instance to the database.
        /// </summary>
        /// <param name="vehicle">Vehicle Class Instance</param>
        /// <returns>Redirect to ContinueBooking action</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Registration,Model,Colour,Travellers")] Vehicle vehicle)
        {
            //Loading in all bookings.
            var allBookings = _context.Bookings.Include(b => b.ApplicationUser);
            
            //Getting first booking related to vehicle.
            var booking = await allBookings.FirstOrDefaultAsync(b => b.Id.Equals(vehicle.Id));

            //Checks if model state is valid.
            if (ModelState.IsValid)
            {
                //Adds vehicle to database and saves.
                _context.Add(vehicle);
                await _context.SaveChangesAsync();

                //Puts vehicles Id into a session.
                HttpContext.Session.SetObjectAsJson("CurrentId", vehicle.Id);

                //Redirects to ContinueBooking action passing the booking class instance.
                return RedirectToAction(nameof(ContinueBooking), booking);
            }

            //Returns view with model.
            return View(vehicle);
        }

        /// <summary>
        /// Calls the Amend View for amending vehicle details.
        /// </summary>
        /// <param name="id">Nullable Integer Value.</param>
        /// <returns>Amending view with model</returns>
        public IActionResult Amend(int? id)
        {
            //Gets vehicle from database using id.
            Vehicle vehicle = _context.Vehicles.Find(id);

            //Gets object from session if previous amend was made on the vehicle in same booking without saving.
            Vehicle amendVehicle = HttpContext.Session.GetObjectFromJson<Vehicle>("VehicleReAmend");

            //Checks if the session is null, if so amendVehicle is set equal to vehicle.
            if(amendVehicle == null)
                amendVehicle = vehicle;

            //Returns view with amendVehicle class instance as model.
            return View(amendVehicle);
        }

        /// <summary>
        /// Post action for amending a vehicle, adds vehicles details to session and returns to AmendBooking action, in the bookingcontroller.
        /// </summary>
        /// <param name="vehicleAmend">Vehicle Class Instance</param>
        /// <returns>Redirect to AmendBooking action on Booking Controller passing id</returns>
        [HttpPost]
        public IActionResult Amend(Vehicle vehicleAmend)
        {
            //Checks that all details are populated, if not adds errormessage text and returns.
            if(vehicleAmend.Colour == null || vehicleAmend.Model == null || vehicleAmend.Registration == null)
            {
                vehicleAmend.ErrorMessage = "All fields must be populated";
                return View(vehicleAmend);
            }

            //Checks if Travellers are present and if not adds errormessage text and returns.
            if(vehicleAmend.Travellers <= 0)
            {
                vehicleAmend.ErrorMessage = "Travellers must be greater than 0";
                return View(vehicleAmend);
            }

            //Gets id into a int variable.
            int id = (int)vehicleAmend.Id;

            //Sets errormessage back to null.
            vehicleAmend.ErrorMessage = null;

            //Adds Vehicle to two different sessions for later use.
            HttpContext.Session.SetObjectAsJson("AmendVehicle", vehicleAmend);
            HttpContext.Session.SetObjectAsJson("VehicleReAmend", vehicleAmend);

            //Redirects to AmendBooking action on bookingcontroller passing id.
            return RedirectToAction(nameof(BookingController.AmendBooking), "Booking", id);
        }

        /// <summary>
        /// Edits a vehicles details during booking process before checkout.
        /// </summary>
        /// <param name="id">Nullable integer value</param>
        /// <returns>Edit View</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            //Checks if id is null and returns NotFound() call if true.
            if (id == null)
            {
                return NotFound();
            }

            //Gets vehicle from databasae.
            var vehicle = await _context.Vehicles.SingleOrDefaultAsync(m => m.Id == id);

            //Checks if vehicle is null, if so returns NotFound() call.
            if (vehicle == null)
            {
                return NotFound();
            }

            //Sets ViewData.
            ViewData["Id"] = new SelectList(_context.Bookings, "Id", "Id", vehicle.Id);

            //Returns Edit View
            return View(vehicle);
        }

        /// <summary>
        /// Edits vehicle details on the database.
        /// </summary>
        /// <param name="id">Nullable integer value</param>
        /// /// <param name="vehicle">Vehicle Class Instance</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Registration,Model,Colour,Travellers")] Vehicle vehicle)
        {
            //Returns NotFound call if ids do not match.
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            //Returns view if no travellers or less.
            if(vehicle.Travellers <= 0)
                return View();

            //Checks if modelstate is valid.
            if (ModelState.IsValid)
            {
                //Tries to update database and save.
                try
                {
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();
                }
                //Catches updating database error.
                catch (DbUpdateConcurrencyException)
                {
                    //If vehicle does not exist returns NotFound call.
                    if (!VehicleExists(vehicle.Id))
                    {
                        return NotFound();
                    }
                    //else throws.
                    else
                    {
                        throw;
                    }
                }
                //Redirects to BookingController.Create Action.
                return RedirectToAction(nameof(BookingController.Create), "Booking");
            }
            //Initialises ViewData.
            ViewData["Id"] = new SelectList(_context.Bookings, "Id", "Id", vehicle.Id);

            //Returns View with model.
            return View(vehicle);
        }

        /// <summary>
        /// Checks if a vehicle exists in the database.
        /// </summary>
        /// <param name="id">Nullable integer value</param>
        /// <returns>True or false</returns>
        private bool VehicleExists(int? id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }

        #region PassingControllers
        /// <summary>
        /// Gets current logged in user using the HttpContext.
        /// </summary>
        /// <returns>ApplicationUser Class Instance</returns>
        public Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        /// <summary>
        /// Redirects method to Checkout action on Booking Controller.
        /// </summary>
        /// <param name="booking">Booking Class Instance</param>
        /// <returns>Checkout Action</returns>
        [AllowAnonymous]
        public ActionResult ContinueBooking(Booking booking)
        {
            return RedirectToAction(nameof(BookingController.Checkout), "Booking", booking);
        }
        #endregion
    }
}
