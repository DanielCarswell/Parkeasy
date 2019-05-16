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

        // GET: Flights/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .Include(f => f.Booking)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
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

        // GET: Flights/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .Include(f => f.Booking)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // POST: Flights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var flight = await _context.Flights.SingleOrDefaultAsync(m => m.Id == id);
            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FlightExists(int? id)
        {
            return _context.Flights.Any(e => e.Id == id);
        }

        #region PassingControllers
        public Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        [AllowAnonymous]
        public ActionResult ContinueBooking(Booking booking)
        {
            return RedirectToAction(nameof(VehicleController.Create), "Vehicle", booking);
        }
        #endregion
    }
}
