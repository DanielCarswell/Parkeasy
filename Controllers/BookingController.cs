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
        [Authorize(Roles = "Admin,Manager,Booking Clerk")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bookings.Include(b => b.ApplicationUser);

            return View(await applicationDbContext.ToListAsync());
        }

        [Authorize(Roles = "Invoice Clerk,Admin,Manager")]
        public IActionResult ValetingStaffIndex()
        {
            var slots = _context.Bookings.Where(b => b.Status == "Delayed" || b.Status == "Booked" || b.Status == "Parked");
            
            return View(slots);
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
            if (booking == null)
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
            string userId = null;
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
            catch (Exception) { }

            if (booking.DepartureDate > DateTime.Now && booking.ReturnDate > booking.DepartureDate)
            {
                int availableSlot = SlotAvailable(booking.DepartureDate, booking.ReturnDate, null);
                if (availableSlot != 0)
                {
                    booking.ApplicationUserId = userId;
                    booking.Duration = (booking.ReturnDate - booking.DepartureDate).Days;
                    booking.Status = "Provisional";
                    booking.SlotId = availableSlot;
                    booking.Price = _context.Pricing.Last().PerDay * (double)booking.Duration;
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ContinueBooking), booking);
                }
                else
                    return View(booking);
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

            var slotId = SlotAvailable(booking.DepartureDate, booking.ReturnDate, booking.Id);

            if (slotId == 0)
                return RedirectToAction(nameof(UnavailableDates));

            if (slotId != booking.SlotId)
                booking.SlotId = slotId;

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
        public async Task<IActionResult> Edit(int id, [Bind("Id,DepartureDate,ReturnDate,Duration,Status,Servicing,ApplicationUserId,PaymentId,ReminderSent,BookedAt,SlotId")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    booking.Duration = (booking.ReturnDate - booking.DepartureDate).Days;
                    booking.Price = _context.Pricing.Last().PerDay * (double)booking.Duration;

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
                return RedirectToAction(nameof(Checkout));
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

        [Authorize(Roles = "Manager")]
        public IActionResult ChangePrice()
        {
            return View(_context.Pricing.Last());
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ChangePrice(Pricing prices)
        {
            if (prices.PerDay != 0 && prices.ServicingCost != 0)
            {
                _context.Pricing.Update(prices);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(prices);
        }

        /// <summary>
        /// Makes sure user is logged in using Authorize DataAnnotation. Has parameter of booking which will be passed
        /// if a user starts a booking before logging in and will then use that Bookings details to create model and pass in.
        /// Otherwise it will attempt to get a booking from database that has been started but not finished by the user
        /// and use that to create the model, if that also does not exist it will make the user start a new booking.
        /// </summary>
        /// <param name="booking">Instance of Booking Class</param>
        /// <returns>Checkout View</returns>
        [AllowAnonymous]
        [Authorize(Roles = "Admin,Manager,Booking Clerk,Customer")]
        public async Task<IActionResult> Checkout(Booking booking)
        {
            ApplicationUser user = null;
            double ServicingCost = 0;

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
                user = await GetCurrentUserAsync();

                //Sets Bookings UserId.
                booking.ApplicationUserId = user?.Id;

                //Updates booking so it now has a UserId.
                _context.Update(booking);

                //Saves changes to database.
                await _context.SaveChangesAsync();
            }
            else
            {
                user = new ApplicationUser();
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

            if(user == null)
                return RedirectToAction(nameof(AccountController.Login), "Account", new{returnUrl = "/Booking/Checkout"});

            //Checks if Servicing is added to booking and adds price if so.
            if (booking.Servicing.Equals(true))
            {
                ServicingCost = (double)_context.Pricing.Last().ServicingCost;
                booking.Price += ServicingCost;
            }

            //Creates instance of BookingViewModel and assigns the property classes to instances above.
            BookingViewModel bookingDetails = new BookingViewModel
            {
                Booking = booking,
                Flight = flight,
                Vehicle = vehicle,
                Charge = (int)(booking.Price + ServicingCost) * 100
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
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Checkout")]
        [Authorize(Roles = "Admin,Manager,Booking Clerk,Customer")]
        public async Task<IActionResult> CheckoutConfirm(string stripeEmail, string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var booking = GetCurrentUserBooking();
            Slot slot = await _context.Slots.Where(s => s.Id == SlotAvailable(booking.DepartureDate, booking.ReturnDate, booking.Id)).FirstOrDefaultAsync();

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

                booking.PaymentId = charge.Id;
                if (charge.Status.ToLower() == "succeeded")
                {
                    await _emailSender.SendEmailAsync(_context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email,
                    "Parkeasy - Booking Successful", "Your booking has been made successfully, you are assigned to Slot " + slot.Id.ToString());

                    Parkeasy.Models.Invoice newInvoice = new Parkeasy.Models.Invoice
                    {
                        Price = booking.Price,
                        InvoiceBody = "Your booking has been made successfully, you are assigned to Slot " + slot.Id.ToString(),
                        InvoiceType = "Successful Payment",
                        Email = _context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email
                    };

                    slot.Status = "Reserved";
                    slot.ToBeAvailable = booking.ReturnDate;
                    slot.Bookings.Add(booking);
                    booking.BookedAt = DateTime.Now;
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

            return RedirectToAction(nameof(UserBookings));
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

            if (user.Result == null)
                return null;

            //Gets Id of the current logged in user.
            string Id = user?.Result.Id;

            if (Id == null)
                return null;

            //Gets list of all bookings.
            var allBookings = _context.Bookings.Include(b => b.ApplicationUser).Where(b => b.Status.Equals("Provisional"));

            //Tries to find a booking specific to logged in user.
            var dbBooking = allBookings.FirstOrDefault(b => b.ApplicationUserId.Equals(Id));

            //Returns instance of Booking class.
            return dbBooking;
        }

        [Authorize]
        public async Task<IActionResult> CancelBooking(int? id)
        {
            Booking booking = new Booking();

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (id == 0)
            {
                return NotFound();
            }

            booking = _context.Bookings.Find(id);

            if (booking.Status == "Provisional")
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Cancelled), booking);
            }

            if (booking.Status == "Booked")
            {
                if (booking.BookedAt.AddDays(2) < DateTime.Now)
                {
                    var options = new RefundCreateOptions
                    {
                        Amount = Convert.ToInt32(booking.Price * 100),
                        Reason = RefundReasons.RequestedByCustomer,
                        ChargeId = booking.PaymentId
                    };

                    var service = new RefundService();
                    Refund refund = service.Create(options);

                    if (refund.Status.ToLower() == "succeeded")
                    {
                        await _emailSender.SendEmailAsync(_context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email,
                    "Parkeasy - Successfully Cancelled", "Your booking has been successfully cancelled and you have been refunded the appropriate amount");

                    Parkeasy.Models.Invoice newInvoice = new Parkeasy.Models.Invoice
                    {
                        Price = booking.Price,
                        InvoiceBody = "Your booking has been successfully cancelled and you have been refunded the appropriate amount",
                        InvoiceType = "Successfully Cancelled",
                        Email = _context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email
                    };

                        _context.Invoices.Add(newInvoice);
                        _context.Bookings.Remove(booking);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Cancelled), booking);
                    }
                }
                else
                {
                    booking.Price = booking.Price * 0.5;
                    var options = new RefundCreateOptions
                    {
                        Amount = Convert.ToInt32(booking.Price * 100),
                        Reason = RefundReasons.RequestedByCustomer,
                        ChargeId = booking.PaymentId
                    };

                    var service = new RefundService();
                    Refund refund = service.Create(options);

                    if (refund.Status.ToLower() == "succeeded")
                    {
                        await _emailSender.SendEmailAsync(_context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email,
                    "Parkeasy - Successfully Cancelled", "Your booking has been successfully cancelled and you have been refunded the appropriate amount");

                    Parkeasy.Models.Invoice newInvoice = new Parkeasy.Models.Invoice
                    {
                        Price = booking.Price,
                        InvoiceBody = "Your booking has been successfully cancelled and you have been refunded the appropriate amount",
                        InvoiceType = "Successfully Cancelled",
                        Email = _context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email
                    };
                        _context.Invoices.Add(newInvoice);
                        _context.Bookings.Remove(booking);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Cancelled), booking);
                    }
                }

            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Cancelled(Booking booking)
        {
            return View(booking);
        }

        #region PassingControllers
        /// <summary>
        /// Gets application user from httpContext.
        /// </summary>
        /// <returns>Instance of Task.Threading.ApplicationUser</returns>
        public Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        /// <summary>
        /// Passes booking details to flights controller to get flight details for booking.
        /// </summary>
        /// <param name="booking">Instance of Booking class.</param>
        /// <returns>Redirect to Flight/Create action.</returns>
        [AllowAnonymous]
        public ActionResult ContinueBooking(Booking booking)
        {
            //Redirects to Create method on FlightController page and passes booking.
            return RedirectToAction(nameof(FlightController.Create), "Flight", booking);
        }

        public IActionResult UnavailableDates()
        {
            return View();
        }
        #endregion

        #region CheckDates

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookingStart">DateTime Variable</param>
        /// <param name="bookingEnd">DateTime Variable</param>
        /// <returns>Integer Value</returns>
        public int SlotAvailable(DateTime bookingStart, DateTime bookingEnd, int? bookingId)
        {
            int days = (int)(bookingEnd - bookingStart).TotalDays;
            var slots = _context.Slots;
            int dayCheck = 0;

            foreach (Slot s in slots)
            {
                if (s.Status == "Delayed")
                    continue;

                if (s.Bookings.Count == 0)
                    s.Bookings = _context.Bookings.Where(b => b.SlotId == s.Id).ToList();

                int count = s.Bookings.Count;
                int checkCount = 0;

                foreach (Booking b in s.Bookings)
                {
                    if(bookingId != null)
                    {
                        if(bookingId == b.Id)
                        {
                            checkCount++;
                            continue;
                        }
                    }
                    for (int i = 0; i <= days - 1; i++)
                    {
                        if (bookingStart.AddDays(i) > b.ReturnDate || bookingStart.AddDays(i) < b.DepartureDate)
                        {
                            dayCheck++;
                        }
                    }
                    if (dayCheck == days)
                        checkCount++;
                    else
                        dayCheck = 0;
                    break;
                }

                if (checkCount == count)
                    return s.Id;
                else
                {
                    checkCount = 0;
                    dayCheck = 0;
                }
            }

            return 0;
        }
        #endregion

        #region UserMethods

        /// <summary>
        /// Displays list of Current Users Bookings.
        /// </summary>
        /// <returns>UserBookings View</returns>
        [Authorize]
        public async Task<IActionResult> UserBookings()
        {
            //Gets all bookings for current user.
            var applicationDbContext = _context.Bookings.Include(b => b.ApplicationUser)
            .Where(b => b.ApplicationUserId == GetCurrentUserAsync().Result.Id);

            //Passes a list of current user bookings to UserBookings View.
            return View(await applicationDbContext.ToListAsync());
        }

        #endregion

        #region ValetingStaff

        public async Task<IActionResult> DelayBooking(int? id)
        {
            Booking booking = _context.Bookings.Find(id);

            booking.Status = "Delayed";
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ValetingStaffIndex));
        }

        public async Task<IActionResult> UndelayBooking(int? id)
        {
            Booking booking = _context.Bookings.Find(id);

            booking.Status = "Parked";
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ValetingStaffIndex));
        }

        public async Task<IActionResult> CompleteBooking(int? id)
        {
            Booking booking = _context.Bookings.Find(id);

            booking.Status = "Complete";
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ValetingStaffIndex));
        }

        public async Task<IActionResult> CheckInBooking(int? id)
        {
            Booking booking = _context.Bookings.Find(id);

            booking.Status = "Parked";
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ValetingStaffIndex));
        }

        #endregion

        #region Amend

        public async Task<IActionResult> AmendBooking(int? id)
        {
            double servicingCost = 0;
            int complexNotNull = 0;
            int flightNotNull = 0;
            int vehicleNotNull = 0;
            int bookingNotNull = 0;
            AmendViewModel amendDetails;

            var myComplexObject = HttpContext.Session.GetObjectFromJson<AmendViewModel>("AmendingFull");

            if (myComplexObject != null)
                complexNotNull = 1;

            if (id == null)
                id = myComplexObject.BookingId;
        

            //Creates instance of BookingViewModel and assigns the property classes to instances above.
            if (complexNotNull == 1)
            {
                amendDetails = myComplexObject;

                var bookingInfo = HttpContext.Session.GetObjectFromJson<Booking>("AmendBooking");
                var flightInfo = HttpContext.Session.GetObjectFromJson<Flight>("AmendFlight");
                var vehicleInfo = HttpContext.Session.GetObjectFromJson<Vehicle>("AmendVehicle");

                if(bookingInfo != null)
                    bookingNotNull = 1;

                if(flightInfo != null)
                    flightNotNull = 1;

                if(vehicleInfo != null)
                    vehicleNotNull = 1;

                if(flightNotNull == 1)
                {
                    amendDetails.Flight.Destination = flightInfo.Destination;
                    amendDetails.Flight.DepartureNumber = flightInfo.DepartureNumber;
                    amendDetails.Flight.ReturnNumber = flightInfo.ReturnNumber;
                }

                if(bookingNotNull == 1)
                {
                    amendDetails.Booking.DepartureDate = bookingInfo.DepartureDate;
                    amendDetails.Booking.ReturnDate = bookingInfo.ReturnDate;
                    amendDetails.Booking.Servicing = bookingInfo.Servicing;
                    amendDetails.Booking.Duration = (int)(bookingInfo.ReturnDate - bookingInfo.DepartureDate).TotalDays;
                }

                if(vehicleNotNull == 1)
                {
                    amendDetails.Vehicle.Model = vehicleInfo.Model;
                    amendDetails.Vehicle.Colour = vehicleInfo.Colour;
                    amendDetails.Vehicle.Registration = vehicleInfo.Registration;
                    amendDetails.Vehicle.Travellers = vehicleInfo.Travellers;
                }
            }
            else
            {

                Booking booking = await _context.Bookings.FindAsync(id);

            //Checks for a flight specific to the booking.
            Flight flight = await _context.Flights.FindAsync(id);

            //Checks for a vehicle specific to the booking.
            Vehicle vehicle = await _context.Vehicles.FindAsync(id);

            //Checks if Servicing is added to booking and adds price if so.
            if (booking.Servicing.Equals(true))
                servicingCost = (double)_context.Pricing.Last().ServicingCost;

                amendDetails = new AmendViewModel
                {
                    Booking = booking,
                    Flight = flight,
                    Vehicle = vehicle,
                    BookingId = booking.Id,
                    FlightId = (int)flight.Id,
                    VehicleId = (int)vehicle.Id,
                    Charge = (int)(booking.Price)
                };
            }

            HttpContext.Session.SetObjectAsJson("AmendingFull", amendDetails);
            //Returns Checkout View with bookingDetails as model.
            return View(amendDetails);
        }

        public async Task<IActionResult> FinishAmending(AmendViewModel amendDetails)
        {
            Booking booking = await _context.Bookings.FindAsync(amendDetails.BookingId);
            Flight flight = await _context.Flights.FindAsync(amendDetails.FlightId);
            Vehicle vehicle = await _context.Vehicles.FindAsync(amendDetails.VehicleId);

            if (amendDetails == null)
            {
                return NotFound();
            }

            if (booking.Status == "Booked")
            {
                if (booking.BookedAt.AddDays(1) < DateTime.Now)
                {
                    var options = new RefundCreateOptions
                    {
                        Amount = Convert.ToInt32(booking.Price * 100),
                        Reason = RefundReasons.RequestedByCustomer,
                        ChargeId = booking.PaymentId
                    };

                    var service = new RefundService();
                    Refund refund = service.Create(options);

                    if (refund.Status.ToLower() == "succeeded")
                    {
                        _context.Bookings.Remove(booking);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Cancelled), booking);
                    }
                }
                else
                {
                    booking.Price = booking.Price * 0.5;
                    var options = new RefundCreateOptions
                    {
                        Amount = Convert.ToInt32(booking.Price * 100),
                        Reason = RefundReasons.RequestedByCustomer,
                        ChargeId = booking.PaymentId
                    };

                    var service = new RefundService();
                    Refund refund = service.Create(options);

                    if (refund.Status.ToLower() == "succeeded")
                    {
                        _context.Bookings.Remove(booking);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Cancelled), booking);
                    }
                }

            }
            return RedirectToAction(nameof(UserBookings));
        }   

        public IActionResult Amend(int? id)
        {
            Booking booking = _context.Bookings.Find(id);
            return View(booking);
        }

        [HttpPost]
        public IActionResult Amend(Booking bookingAmend)
        {
            int id = bookingAmend.Id;
            HttpContext.Session.SetObjectAsJson("AmendBooking", bookingAmend);
            return RedirectToAction(nameof(AmendBooking), id);
        }

        #endregion
    }
}