using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Parkeasy.Data;
using Parkeasy.Models;
using Parkeasy.Models.BookingViewModels;

namespace Parkeasy.Controllers
{
    public class FlightController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FlightController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Flights
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Flights.Include(f => f.Booking);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Flights/Create
        public IActionResult Create(Booking booking)
        {
            Flight flight = new Flight
            {
                Booking = booking,
                Id = booking.Id,
            };
            return View(flight);
        }

        // POST: Flights/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartureNumber,ReturnNumber,DepartureDateTime,ReturnDateTime,Destination")] Flight flight)
        {
            var allBookings = _context.Bookings.Include(b => b.ApplicationUser);
            var booking = await allBookings.FirstOrDefaultAsync(b => b.Id.Equals(flight.Id));
            
            if (ModelState.IsValid)
            {
                flight.DepartureDateTime = booking.DepartureDate;
                flight.ReturnDateTime = booking.DepartureDate;
                _context.Add(flight);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ContinueBooking), flight.Booking);
            }
            return View(flight);
        }

        public IActionResult Amend(int? id)
        {
            Flight flight = _context.Flights.Find(id);
            Flight amendFlight = HttpContext.Session.GetObjectFromJson<Flight>("FlightReAmend");
            if(amendFlight == null)
                amendFlight = flight;
            return View(amendFlight);
        }

        [HttpPost]
        public IActionResult Amend(Flight flightAmend)
        {
            int id = (int)flightAmend.Id;
            HttpContext.Session.SetObjectAsJson("AmendFlight", flightAmend);
            HttpContext.Session.SetObjectAsJson("FlightReAmend", flightAmend);
            return RedirectToAction(nameof(BookingController.AmendBooking), "Booking", id);
        }

        // GET: Flights/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights.SingleOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Bookings, "Id", "Id", flight.Id);
            return View(flight);
        }

        // POST: Flights/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,DepartureNumber,ReturnNumber,DepartureDateTime,ReturnDateTime,Destination")] Flight flight)
        {
            if (id != flight.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlightExists(flight.Id))
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
            ViewData["Id"] = new SelectList(_context.Bookings, "Id", "Id", flight.Id);
            return View(flight);
        }

        /// <summary>
        /// Checks if a Flight exists in database using its Id.
        /// </summary>
        /// <param name="id">Nullable Integer Value</param>
        /// <returns>Boolean Value (True/False)</returns>
        private bool FlightExists(int? id)
        {
            return _context.Flights.Any(e => e.Id == id);
        }

        #region PassingControllers

        /// <summary>
        /// Gets the current user from HttpContext.
        /// </summary>
        /// <returns>Instance of ApplicationUser class.</returns>
        public Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        /// <summary>
        /// Passes booking class instance to vehicle booking process.
        /// </summary>
        /// <param name="booking">Instance of Booking class</param>
        /// <returns>Redirect to Vehicle create action</returns>
        [AllowAnonymous]
        public ActionResult ContinueBooking(Booking booking)
        {
            return RedirectToAction(nameof(VehicleController.Create), "Vehicle", booking);
        }
        #endregion
    }
}
