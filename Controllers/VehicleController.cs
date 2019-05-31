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

namespace Parkeasy.Controllers
{
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
            Vehicle vehicle = new Vehicle
            {
                Booking = booking,
                Id = booking.Id,
            };
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
            var allBookings = _context.Bookings.Include(b => b.ApplicationUser);
            var booking = await allBookings.FirstOrDefaultAsync(b => b.Id.Equals(vehicle.Id));

            if (ModelState.IsValid)
            {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetObjectAsJson("CurrentId", vehicle.Id);
                return RedirectToAction(nameof(ContinueBooking), booking);
            }
            return View(vehicle);
        }

        public IActionResult Amend(int? id)
        {
            Vehicle vehicle = _context.Vehicles.Find(id);
            Vehicle amendVehicle = HttpContext.Session.GetObjectFromJson<Vehicle>("VehicleReAmend");
            if(amendVehicle == null)
                amendVehicle = vehicle;
            return View(amendVehicle);
        }

        [HttpPost]
        public IActionResult Amend(Vehicle vehicleAmend)
        {
            int id = (int)vehicleAmend.Id;
            HttpContext.Session.SetObjectAsJson("AmendVehicle", vehicleAmend);
            HttpContext.Session.SetObjectAsJson("VehicleReAmend", vehicleAmend);
            return RedirectToAction(nameof(BookingController.AmendBooking), "Booking", id);
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.SingleOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Bookings, "Id", "Id", vehicle.Id);
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Registration,Model,Colour,Travellers")] Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            if(vehicle.Travellers <= 0)
                return View();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(BookingController.Create), "Booking");
            }
            ViewData["Id"] = new SelectList(_context.Bookings, "Id", "Id", vehicle.Id);
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
