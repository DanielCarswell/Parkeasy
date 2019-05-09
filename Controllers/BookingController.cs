using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Parkeasy.Data;
using Parkeasy.Models;
using Parkeasy.Models.BookingViewModels;
using Stripe;
using Parkeasy.Services;
using System.Security.Claims;

namespace Parkeasy.Controllers
{
    /// <summary>
    /// Handles operations involving Booking data.
    /// </summary>
    public class BookingController : Controller
    {
        /// <summary>
        /// Global variables for Database Context and UserManager.
        /// </summary>
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Constructor for initialising global variable data.
        /// </summary>
        /// <param name="context">Instance of ApplicationDbContext Class.</param>
        /// <param name="userManager">Instance of UserManager Class with ApplicationUser Type.</param>
        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            //Sets globals equal to passed in instances.
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Displays list of all Bookings.
        /// </summary>
        /// <returns>Index View</returns>
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bookings.Include(b => b.ApplicationUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.ApplicationUser)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Booking/Create
        public IActionResult Create()
        {
            var booking = GetCurrentUserBooking();
            if(booking == null)
                return View();
            else
                return RedirectToAction(nameof(Checkout));
        }

        // POST: Booking/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (booking.DepartureDate > DateTime.Now && booking.ReturnDate > booking.DepartureDate)
            {
                booking.Duration = (booking.ReturnDate - booking.DepartureDate).Days;
                booking.Status = "Provisional";
                booking.Price = 10.00 * (double)booking.Duration;
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ContinueBooking), booking);
            }
            else
                return View(booking);
        }

        // GET: Booking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.SingleOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", booking.ApplicationUserId);
            return View(booking);
        }

        // POST: Booking/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DepartureDate,ReturnDate,Duration,Status,ApplicationUserId,PaymentId")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
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
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", booking.ApplicationUserId);
            return View(booking);
        }

        // GET: Booking/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.ApplicationUser)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.SingleOrDefaultAsync(m => m.Id == id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }

        /// <summary>
        /// Makes sure user is logged in using Authorize DataAnnotation. Has parameter of booking which will be passed
        /// if a user starts a booking before logging in and will then use that Bookings details to create model and pass in.
        /// Otherwise it will attempt to get a booking from database that has been started but not finished by the user
        /// and use that to create the model, if that also does not exist it will make the user start a new booking.
        /// </summary>
        /// <param name="booking">Instance of Booking Class</param>
        /// <returns>Checkout View</returns>
        [Authorize]
        public async Task<IActionResult> Checkout(Booking booking)
        {
            //Checks if passed in booking is null.
            if (booking.Duration == 0 || booking.Id == 0)
            {
                //Gets instance of Booking from database that a User has started but not finished.
                booking = GetCurrentUserBooking();

                //Checks again if booking is null after running method reference above.
                if (booking == null)
                    //Redirects to create a booking, if booking is still null.
                    return RedirectToAction(nameof(Create));
            }

            //Checks if the booking does not have a User Id, this essentially means if the Booking was made
            //Whilst not logged in, then it will assign that booking to the user.
            if (booking.ApplicationUserId == null)
            {
                //Gets user asynchronously.
                var user = await GetCurrentUserAsync();

                //Sets Bookings UserId.
                booking.ApplicationUserId = user?.Id;

                //Updates booking so it now has a UserId.
                _context.Update(booking);

                //Saves changes to database.
                await _context.SaveChangesAsync();
            }

            //Gets list of all Flights.
            var allFlights = _context.Flights.Include(b => b.Booking);

            //Checks for a flight specific to the booking.
            var flight = await allFlights.FirstOrDefaultAsync(f => f.Id.Equals(booking.Id));

            //If no flight assigned to booking redirects user to create one.
            if (flight == null)
                return RedirectToAction(nameof(FlightController.Create), "Flight", booking);

            //Gets list of all Vehicles.
            var allVehicles = _context.Vehicles.Include(b => b.Booking);

            //Checks for a vehicle specific to the booking.
            var vehicle = await allVehicles.FirstOrDefaultAsync(v => v.Id.Equals(booking.Id));

            //If no vehicle assigned to booking redirects user to create one.
            if (vehicle == null)
                return RedirectToAction(nameof(VehicleController.Create), "Vehicle", booking);

            //Creates instance of BookingViewModel and assigns the property classes to instances above.
            BookingViewModel bookingDetails = new BookingViewModel
            {
                Booking = booking,
                Flight = flight,
                Vehicle = vehicle,
                Charge = (int)booking.Price * 100
            };

            //Returns Checkout View with bookingDetails as model.
            return View(bookingDetails);
        }

        /// <summary>
        /// Checks if payment was successful and if so will send an email with details, add invoice details to database
        /// And update Booking and Slot tables on database.
        /// </summary>
        /// <param name="stripeEmail">String Variable</param>
        /// <param name="stripeToken">String Variable</param>
        /// <returns>Redirect to Homepage</returns>
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Checkout")]
        public async Task<IActionResult> CheckoutConfirm(string stripeEmail, string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var booking = GetCurrentUserBooking();
            Slot slot = GetAvailableSlot();

            if (slot.Id == 0 || slot == null)
                return View();

            //Stripe Logic
            if (stripeToken != null)
            {
                var customers = new CustomerService();
                var charges = new ChargeService();

                var customer = customers.Create(new CustomerCreateOptions
                {
                    Email = stripeEmail,
                    SourceToken = stripeToken
                });

                var charge = charges.Create(new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(booking.Price * 100),
                    Description = "Booking Id: " + booking.Id,
                    Currency = "gbp",
                    CustomerId = customer.Id
                });

                booking.PaymentId = charge.BalanceTransactionId;
                if (charge.Status.ToLower() == "succeeded")
                {
                    await _emailSender.SendEmailAsync(_context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email,
                    "Parkeasy - Booking Successful", "Your booking has been made successfully, you are assigned to Slot " + slot.Id.ToString());

                    Parkeasy.Models.Invoice newInvoice = new Parkeasy.Models.Invoice
                    {
                        Price = booking.Price,
                        InvoiceBody = "Your booking has been made successfully, you are assigned to Slot " + slot.Id.ToString(),
                        InvoiceType = "Successful Payment"
                    };

                    slot.Status = "Reserved";
                    slot.ToBeAvailable = booking.ReturnDate;
                    slot.LastBookingId = booking.Id;

                    booking.Status = "Booked";
                    _context.Update(booking);
                    _context.Update(slot);
                    _context.Invoices.Add(newInvoice);

                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                return View();
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /// <summary>
        /// Gets Current Logged In Users Id and attempts to find an unfinished Booking related to that user.await
        /// And return that booking.
        /// </summary>
        /// <returns>Instance of Booking Class</returns>
        public Booking GetCurrentUserBooking()
        {
            //Gets current logged in user.
            var user = GetCurrentUserAsync();

            if(user.Result == null)
                return null;

            //Gets Id of the current logged in user.
            string Id = user?.Result.Id;

            if(Id == null)
                return null;

            //Gets list of all bookings.
            var allBookings = _context.Bookings.Include(b => b.ApplicationUser).Where(b => b.Status.Equals("Provisional"));

            //Tries to find a booking specific to logged in user.
            var dbBooking = allBookings.FirstOrDefault(b => b.ApplicationUserId.Equals(Id));

            //Returns instance of Booking class.
            return dbBooking;
        }

        /// <summary>
        /// Gets next avaiable Slot in Car Park.
        /// </summary>
        /// <returns>Instance of Slot Class</returns>
        public Slot GetAvailableSlot()
        {
            //Gets all Slot data from Database using ApplicationDbContext.
            var allSlots = _context.Slots;

            //Attempts to get a slot that has an available status.
            var slot = _context.Slots.FirstOrDefault(s => s.Status.Equals("Available"));

            //If slot is null, returns null.
            if (slot == null)
                return null;

            //If slot exists, returns the Slot Instance.
            else
                return slot;
        }

        #region PassingControllers
        public Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        [AllowAnonymous]
        public ActionResult ContinueBooking(Booking booking)
        {
            return RedirectToAction(nameof(FlightController.Create), "Flight", booking);
        }
        #endregion
    }
}
