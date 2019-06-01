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
using Parkeasy.Models.Reports;
using Parkeasy.Models.BookingViewModels;
using Stripe;
using Parkeasy.Services;
using System.Security.Claims;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;
using DinkToPdf;
using System.IO;
using DinkToPdf.Contracts;

namespace Parkeasy.Controllers
{
    /// <summary>
    /// Handles operations involving Booking data.
    /// </summary>
    public class BookingController : Controller
    {
        /// <summary>
        /// Global variables for Database Context, EmailSender, UserManager and IConverter.
        /// </summary>
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConverter _converter;

        /// <summary>
        /// Constructor for initialising global variable data.
        /// </summary>
        /// <param name="context">Instance of ApplicationDbContext Class.</param>
        /// <param name="userManager">Instance of UserManager Class with ApplicationUser Type.</param>
        /// /// <param name="emailSender">Instance of IEmailSender Interface.</param>
        /// /// <param name="converter">Instance of IConverter Interface.</param>
        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IConverter converter)
        {
            //Sets globals equal to passed in instances.
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _converter = converter;
            //GenerateReports();
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

        /// <summary>
        /// Makes a SelectList of Report types and passes it to a view for generating report based on chosen dropdown.
        /// </summary>
        /// <returns>Redirect to Reports View passing in model</returns>
        [Authorize(Roles = "Admin,Manager,Valeting Staff,Booking Clerk,Invoice Clerk")]
        public IActionResult Reports()
        {
            List<ReportViewModel> reports = new List<ReportViewModel>();

            //Add to List of ReportViewModel class instances.
            reports.Add(new ReportViewModel { Report = "Booking Report" });
            reports.Add(new ReportViewModel { Report = "Release Report" });
            reports.Add(new ReportViewModel { Report = "Valeting Report" });
            reports.Add(new ReportViewModel { Report = "Monthly Bookings Report" });
            reports.Add(new ReportViewModel { Report = "Monthly Turnover Report" });

            //Creates ViewData "Report" for displaying Report Types in View.
            ViewData["Report"] = new SelectList(reports, "Report", "Report");

            ReportViewModel model = new ReportViewModel { Report = "Booking Report" };
            return View(model);
        }

        /// <summary>
        /// Passes in a model with a Report type for generating pdf report of.
        /// </summary>
        /// <param name="model">Instance of ReportViewModel Instance.</param>
        /// <returns>Redirects to CreatePDF action passing in Report type string variable</returns>
        [HttpPost]
        public IActionResult Reports(ReportViewModel model)
        {
            //Sets session incase parameter doesnt pass properly.
            HttpContext.Session.SetObjectAsJson("ReportChoice", model.Report);
            return RedirectToAction(nameof(BookingController.CreatePDF), "Booking", model.Report);
        }

        /// <summary>
        /// Displays list of bookings that are due to arrive or are parked/delayed.
        /// </summary>
        /// <returns>View passing in booking data.</returns>
        [Authorize(Roles = "Valeting Staff,Admin,Manager")]
        public IActionResult ValetingStaffIndex()
        {
            var bookings = _context.Bookings.Where(b => b.Status == "Delayed" || b.Status == "Booked" || b.Status == "Parked").Include(b => b.ApplicationUser);

            return View(bookings);
        }

        /// <summary>
        /// Redirect to Booking Create View.
        /// </summary>
        /// <returns>Create View or Redirect to Checkout Action</returns>
        [AllowAnonymous]
        public IActionResult Create()
        {
            var booking = GetCurrentUserBooking();
            if (booking == null)
                return View();
            else
                return RedirectToAction(nameof(Checkout));
        }

        /// <summary>
        /// Adds Booking to database if valid and redirects to ContinueBooking action.
        /// </summary>
        /// <param name="booking">Instance of Booking class.</param>
        /// <returns>Create View if fails, else ContinueBooking action redirect.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            string userId = null;
            try
            {
                //Tries to get userid, only works if a user is logged in.
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
            //Catches an Exception caused by user not being logged in.
            catch (Exception) { }

            //Checks if booking is being made for future and isnt attempting to book for past.
            if (booking.DepartureDate > DateTime.Now && booking.ReturnDate > booking.DepartureDate)
            {
                //Gets a slot id of a slot that is available if any.
                int availableSlot = SlotAvailable(booking.DepartureDate, booking.ReturnDate, null);

                //Checks that slot was returned and not 0.
                if (availableSlot != 0)
                {
                    //Populates booking class then adds it to the database and saves.
                    booking.ApplicationUserId = userId;
                    booking.Duration = (booking.ReturnDate - booking.DepartureDate).Days;
                    booking.Status = "Provisional";
                    booking.SlotId = availableSlot;
                    booking.Price = _context.Pricing.Last().PerDay * (double)booking.Duration;
                    _context.Add(booking);
                    await _context.SaveChangesAsync();

                    //Redirects to ContinueBooking action passing the Booking class instance.
                    return RedirectToAction(nameof(ContinueBooking), booking);
                }
                else
                    //Returns the Create view with Booking class model.
                    return View(booking);
            }
            else
                //Returns the Create view with Booking class model.
                return View(booking);
        }

        /// <summary>
        /// Gets booking for editting using id and passes to Edit View.
        /// </summary>
        /// <param name="id">nullable int variable</param>
        /// <returns>Returns Edit View</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            //Returns not found if id is null.
            if (id == null)
            {
                return NotFound();
            }

            //Gets booking and checks if a slot is available returning a slotid if so.
            var booking = await _context.Bookings.SingleOrDefaultAsync(m => m.Id == id);
            var slotId = SlotAvailable(booking.DepartureDate, booking.ReturnDate, booking.Id);

            //Check if slotid is null and returns UnavailableDates action if so.
            if (slotId == 0)
                return RedirectToAction(nameof(UnavailableDates));

            //Changes slotId if doesnt match new slotid found.
            if (slotId != booking.SlotId)
                booking.SlotId = slotId;

            //Checks if booking is null, returns call to NotFound() if true. 
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", booking.ApplicationUserId);

            //returns Edit View passing in Booking class instance.
            return View(booking);
        }

        /// <summary>
        /// Edits booking details in the database.
        /// </summary>
        /// <param name="id">integer value</param>
        /// <param name="booking">Instance of Booking Class</param>
        /// <returns>Redirect to checkout action or Edit View if fails.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DepartureDate,ReturnDate,Duration,Status,Servicing,ApplicationUserId,PaymentId,ReminderSent,BookedAt,SlotId")] Booking booking)
        {
            //Checks that the ids match.
            if (id != booking.Id)
            {
                return NotFound();
            }

            //Runs if modelstate is valid
            if (ModelState.IsValid)
            {
                try
                {
                    //Updates some booking details.
                    booking.Duration = (booking.ReturnDate - booking.DepartureDate).Days;
                    booking.Price = _context.Pricing.Last().PerDay * (double)booking.Duration;

                    //Updates booking and saves.
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                //Catches database updating error.
                catch (DbUpdateConcurrencyException)
                {
                    //Checks if booking doesnt exist.
                    if (!BookingExists(booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //Redirects to Checkout action.
                return RedirectToAction(nameof(Checkout));
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", booking.ApplicationUserId);

            //Returns Edit View.
            return View(booking);
        }

        /// <summary>
        /// Checks if booking exists.
        /// </summary>
        /// <param name="id">Integer Value</param>
        /// <returns>Boolean(True or False)</returns>
        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }

        /// <summary>
        /// Returns a view populated with latest pricing costs.
        /// </summary>
        /// <returns>ChangePrice View</returns>
        [Authorize(Roles = "Manager")]
        public IActionResult ChangePrice()
        {
            return View(_context.Pricing.Last());
        }

        /// <summary>
        /// Updates Prices in database.
        /// </summary>
        /// <param name="prices">Pricing Class Instance.</param>
        /// <returns>Redirect to Index action</returns>
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
            //Initialising local variables.
            ApplicationUser user = null;
            double ServicingCost = 0;

            //Checks if passed in booking is null.
            if (booking.Duration == 0 || booking.Id == 0)
            {
                //Gets instance of Booking from database that a User has started but not finished.
                booking = GetCurrentUserBooking();

                if (booking == null)
                {
                    booking = _context.Bookings.Find(HttpContext.Session.GetObjectFromJson<int>("CurrentId"));
                }

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

            if (user == null)
                return RedirectToAction(nameof(AccountController.Login), "Account", new { returnUrl = "/Booking/Checkout" });

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
                Charge = (int)(booking.Price) * 100
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
            //Gets Claim class instance to handle logged in user.
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //Gets current user booking and gets an available slot.
            var booking = GetCurrentUserBooking();
            Slot slot = await _context.Slots.Where(s => s.Id == SlotAvailable(booking.DepartureDate, booking.ReturnDate, booking.Id)).FirstOrDefaultAsync();

            //Returns view if slot is 0 or null.
            if (slot.Id == 0 || slot == null)
                return View();

            //Stripe Logic
            if (stripeToken != null)
            {
                //Initialising local variables.
                var customers = new CustomerService();
                var charges = new ChargeService();
                var customer = customers.Create(new CustomerCreateOptions
                {
                    Email = stripeEmail,
                    SourceToken = stripeToken
                });

                //Creating ChargeCreateOptions for charging user.
                var charge = charges.Create(new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(booking.Price * 100),
                    Description = "Booking Id: " + booking.Id,
                    Currency = "gbp",
                    CustomerId = customer.Id
                });

                //Gets id for payment.
                booking.PaymentId = charge.Id;

                //Checks if payment was made successfully.
                if (charge.Status.ToLower() == "succeeded")
                {
                    //Sends Invoice
                    await _emailSender.SendEmailAsync(_context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email,
                    "Parkeasy - Booking Successful", "Your booking has been made successfully, you are assigned to Slot " + slot.Id.ToString());

                    //Uncomment below code to enable sms messaging, trial account has limited uses.
                    /*
                    const string accountSid = "ACbd58a3f163cd7bae7679006f001fbf55";  
                    const string authToken = "3df899d9bc91bdf5bc128dace411fe93";  
                    TwilioClient.Init(accountSid, authToken);  
   
                    var to = new PhoneNumber(_context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().PhoneNumber);  
                    var message = MessageResource.Create(  
                        to,  
                        from: new PhoneNumber("+447723469919"), //  From number, must be an SMS-enabled Twilio number ( This will send sms from ur "To" numbers ).  
                        body:  "Your booking has been made successfully, you are assigned to Slot " + slot.Id.ToString()); 

                    */

                    //Creates invoice for adding to database.
                    Parkeasy.Models.Invoice newInvoice = new Parkeasy.Models.Invoice
                    {
                        Price = booking.Price,
                        InvoiceBody = "Your booking has been made successfully, you are assigned to Slot " + slot.Id.ToString(),
                        InvoiceType = "Successful Payment",
                        Email = _context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email
                    };

                    //Updates data in class instances for booking and slot, updates them in database.
                    slot.Status = "Reserved";
                    slot.ToBeAvailable = booking.ReturnDate;
                    slot.Bookings.Add(booking);
                    booking.BookedAt = DateTime.Now;
                    booking.Status = "Booked";
                    _context.Update(booking);
                    _context.Update(slot);
                    _context.Invoices.Add(newInvoice);

                    //Saves changes to the database.
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                //Returns Checkout View.
                return View();
            }

            //Returns redirect to UserBookings Action.
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

        /// <summary>
        /// Cancels users booking.
        /// </summary>
        /// <param name="id">Nullable integer value.</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> CancelBooking(int? id)
        {
            //Initialising local variable.
            Booking booking = new Booking();

            //Gets claim for handling logged in user.
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //Returns notfound if id is null or 0.
            if (id == 0 || id == null)
            {
                return NotFound();
            }

            //Gets booking from database.
            booking = _context.Bookings.Find(id);

            //If booking status is provisional will remove booking, save changes to database.
            //And redirect to Cancelled action.
            if (booking.Status == "Provisional")
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Cancelled), booking);
            }

            //If booking is booked.
            if (booking.Status == "Booked")
            {
                //Checks if it has been 2 days since booking was made.
                if (booking.BookedAt.AddDays(2) < DateTime.Now)
                {
                    //Creates new options for refunding user.
                    var options = new RefundCreateOptions
                    {
                        Amount = Convert.ToInt32(booking.Price * 100),
                        Reason = RefundReasons.RequestedByCustomer,
                        ChargeId = booking.PaymentId
                    };

                    //Runs refund service.
                    var service = new RefundService();
                    Refund refund = service.Create(options);

                    //If refunding was successful.
                    if (refund.Status.ToLower() == "succeeded")
                    {
                        //Sends invoice
                        await _emailSender.SendEmailAsync(_context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email,
                    "Parkeasy - Successfully Cancelled", "Your booking has been successfully cancelled and you have been refunded the appropriate amount");

                    //Creates invoice for database
                        Parkeasy.Models.Invoice newInvoice = new Parkeasy.Models.Invoice
                        {
                            Price = booking.Price,
                            InvoiceBody = "Your booking has been successfully cancelled and you have been refunded the appropriate amount",
                            InvoiceType = "Successfully Cancelled",
                            Email = _context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email
                        };

                        //Adds invoice to database, removes booking and saves changes.
                        _context.Invoices.Add(newInvoice);
                        _context.Bookings.Remove(booking);
                        await _context.SaveChangesAsync();

                        //Redirects to Cancelled action.
                        return RedirectToAction(nameof(Cancelled), booking);
                    }
                }
                else
                {
                    //Halves booking price for only refunding half.
                    booking.Price = booking.Price * 0.5;

                    //Creates refunding option.
                    var options = new RefundCreateOptions
                    {
                        Amount = Convert.ToInt32(booking.Price * 100),
                        Reason = RefundReasons.RequestedByCustomer,
                        ChargeId = booking.PaymentId
                    };

                    //Runs service for refunding user.
                    var service = new RefundService();
                    Refund refund = service.Create(options);

                    //Changes refund status to success.
                    if (refund.Status.ToLower() == "succeeded")
                    {
                        //Sends invoice to user.
                        await _emailSender.SendEmailAsync(_context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email,
                    "Parkeasy - Successfully Cancelled", "Your booking has been successfully cancelled and you have been refunded the appropriate amount");

                        //Creates invoice for database.
                        Parkeasy.Models.Invoice newInvoice = new Parkeasy.Models.Invoice
                        {
                            Price = booking.Price,
                            InvoiceBody = "Your booking has been successfully cancelled and you have been refunded the appropriate amount",
                            InvoiceType = "Successfully Cancelled",
                            Email = _context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email
                        };

                        //Adds invoice to database, removes booking and saves changes.
                        _context.Invoices.Add(newInvoice);
                        _context.Bookings.Remove(booking);
                        await _context.SaveChangesAsync();

                        //Redirects to cancelled action.
                        return RedirectToAction(nameof(Cancelled), booking);
                    }
                }

            }
            //Redirects to index action.
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Returs view for confirming cancelled booking.
        /// </summary>
        /// <param name="booking">booking class instance.</param>
        /// <returns>Returns Cancelled View.</returns>
        [Authorize]
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

        /// <summary>
        /// Returns UnavailableDates view.
        /// </summary>
        /// <returns>Returns UnavailableDates View.</returns>
        public IActionResult UnavailableDates()
        {
            return View();
        }
        #endregion

        #region CheckDates

        /// <summary>
        /// Calculates if any slots are available for the appropriate dates.
        /// </summary>
        /// <param name="bookingStart">DateTime Variable</param>
        /// <param name="bookingEnd">DateTime Variable</param>
        /// <returns>Integer Value</returns>
        public int SlotAvailable(DateTime bookingStart, DateTime bookingEnd, int? bookingId)
        {
            //Initialising Local Variables.
            int days = (int)(bookingEnd - bookingStart).TotalDays;
            var slots = _context.Slots;
            int dayCheck = 0;

            //Repeats for all slots.
            foreach (Slot s in slots)
            {
                //Continues loop if slot status is currently Delayed.
                if (s.Status == "Delayed")
                    continue;

                //Gets bookings if they did not load in successfully.
                if (s.Bookings.Count == 0)
                    s.Bookings = _context.Bookings.Where(b => b.SlotId == s.Id).ToList();

                //Initialising local variables.
                int count = s.Bookings.Count;
                int checkCount = 0;

                //Repeating for all bookings in the Slots Bookings list.
                foreach (Booking b in s.Bookings)
                {
                    //Runs if bookingId is not null.
                    if (bookingId != null)
                    {
                        //Checks if bookings Ids match.
                        if (bookingId == b.Id)
                        {
                            //Increments checkCount and skips to next iteration of loop.
                            checkCount++;
                            continue;
                        }
                    }
                    //Loops for all days.
                    for (int i = 0; i <= days - 1; i++)
                    {
                        //Checks if day is suitable.
                        if (bookingStart.AddDays(i) > b.ReturnDate || bookingStart.AddDays(i) < b.DepartureDate)
                        {
                            dayCheck++;
                        }
                    }
                    //Checks if all days were available.
                    if (dayCheck == days)
                        checkCount++;
                    else
                        dayCheck = 0;
                    break;
                }
                //Checks if checkCount matches count and returns available slots id if so.
                if (checkCount == count)
                    return s.Id;
                //Resets variables for next iteration.
                else
                {
                    checkCount = 0;
                    dayCheck = 0;
                }
            }

            //Returns 0.
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

        /// <summary>
        /// Changes Booking Status to delayed.
        /// </summary>
        /// <param name="id">Nullable Integer Value</param>
        /// <returns>Redirect to ValetingStaffIndex</returns>
        public async Task<IActionResult> DelayBooking(int? id)
        {
            Booking booking = _context.Bookings.Find(id);

            booking.Status = "Delayed";
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ValetingStaffIndex));
        }

        /// <summary>
        /// Changes Booking Status to Parked.
        /// </summary>
        /// <param name="id">Nullable Integer Value</param>
        /// <returns>Redirect to ValetingStaffIndex</returns>
        public async Task<IActionResult> UndelayBooking(int? id)
        {
            Booking booking = _context.Bookings.Find(id);

            booking.Status = "Parked";
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ValetingStaffIndex));
        }

        /// <summary>
        /// Changes Booking Status to Complete.
        /// </summary>
        /// <param name="id">Nullable Integer Value</param>
        /// <returns>Redirect to ValetingStaffIndex</returns>
        public async Task<IActionResult> CompleteBooking(int? id)
        {
            Booking booking = _context.Bookings.Find(id);

            booking.Status = "Complete";
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetObjectAsJson("ReleaseBookingId", id);
            return RedirectToAction(nameof(AddReleaseReport));
        }

        /// <summary>
        /// Changes Booking Status to Parked.
        /// </summary>
        /// <param name="id">Nullable Integer Value</param>
        /// <returns>Redirect to ValetingStaffIndex</returns>
        public async Task<IActionResult> CheckInBooking(int? id)
        {
            Booking booking = _context.Bookings.Find(id);

            booking.Status = "Parked";
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetObjectAsJson("BookingId", id);
            return RedirectToAction(nameof(AddBookingReport));
        }
        /// <summary>
        /// Changes bookings Valet Property to false removing it from list of needing valeted.
        /// </summary>
        /// <param name="id">Nullable Integer Value</param>
        /// <returns>Redirect to AddValetingReport action</returns>
        public async Task<IActionResult> ValetingComplete(int? id)
        {
            Booking booking = _context.Bookings.Find(id);

            booking.Valeting = false;
            await _context.SaveChangesAsync();

            HttpContext.Session.SetObjectAsJson("ValetBookingId", id);
            return RedirectToAction(nameof(AddValetingReport));
        }

        #endregion

        #region Amend

        public async Task<IActionResult> AmendBooking(int? id)
        {
            //Initialising local variables.
            double servicingCost = 0;
            int complexNotNull = 0;
            int flightNotNull = 0;
            int vehicleNotNull = 0;
            int bookingNotNull = 0;
            AmendViewModel amendDetails;

            //Gets object from session.
            var myComplexObject = HttpContext.Session.GetObjectFromJson<AmendViewModel>("AmendingFull");

            //Checks if object is not null and if so sets NotNull to 1.
            if (myComplexObject != null)
                complexNotNull = 1;

            //Checks if id is null, if so gets id from Object.
            if (id == null)
                id = myComplexObject.BookingId;

            //Checks if complexNotNull is 1.
            if (complexNotNull == 1)
            {
                //Sets amendDetails to object.
                amendDetails = myComplexObject;

                //Gets details from sessions.
                var bookingInfo = HttpContext.Session.GetObjectFromJson<Booking>("AmendBooking");
                var flightInfo = HttpContext.Session.GetObjectFromJson<Flight>("AmendFlight");
                var vehicleInfo = HttpContext.Session.GetObjectFromJson<Vehicle>("AmendVehicle");

                //Checks if any is null and assigns values to variables accordingly.
                if (bookingInfo != null)
                    bookingNotNull = 1;

                if (flightInfo != null)
                    flightNotNull = 1;

                if (vehicleInfo != null)
                    vehicleNotNull = 1;

                //Changes flight details variable equal to 1.
                if (flightNotNull == 1)
                {
                    amendDetails.Flight.Destination = flightInfo.Destination;
                    amendDetails.Flight.DepartureNumber = flightInfo.DepartureNumber;
                    amendDetails.Flight.ReturnNumber = flightInfo.ReturnNumber;
                }

                //Changes booking details variable equal to 1.
                if (bookingNotNull == 1)
                {
                    amendDetails.Booking.DepartureDate = bookingInfo.DepartureDate;
                    amendDetails.Booking.ReturnDate = bookingInfo.ReturnDate;
                    amendDetails.Booking.Servicing = bookingInfo.Servicing;
                    amendDetails.Booking.Duration = (int)(bookingInfo.ReturnDate - bookingInfo.DepartureDate).TotalDays;
                }

                //Changes vehicle details variable equal to 1.
                if (vehicleNotNull == 1)
                {
                    amendDetails.Vehicle.Model = vehicleInfo.Model;
                    amendDetails.Vehicle.Colour = vehicleInfo.Colour;
                    amendDetails.Vehicle.Registration = vehicleInfo.Registration;
                    amendDetails.Vehicle.Travellers = vehicleInfo.Travellers;
                }
            }
            else
            {
                //Gets booking from database.
                Booking booking = await _context.Bookings.FindAsync(id);

                //Checks for a flight specific to the booking.
                Flight flight = await _context.Flights.FindAsync(id);

                //Checks for a vehicle specific to the booking.
                Vehicle vehicle = await _context.Vehicles.FindAsync(id);

                //Checks if Servicing is added to booking and adds price if so.
                if (booking.Servicing.Equals(true))
                    servicingCost = (double)_context.Pricing.Last().ServicingCost;

                //Sets amendDetails equal to new Instance of AmendViewModel class.
                amendDetails = new AmendViewModel
                {
                    Booking = booking,
                    Flight = flight,
                    Vehicle = vehicle,
                    BookingId = booking.Id,
                    FlightId = (int)flight.Id,
                    VehicleId = (int)vehicle.Id,
                    Charge = 0
                };
            }

            //Gets booking.
            Booking getBooking = await _context.Bookings.FindAsync(id);

            //Sets price according to days since booking, difference in days of booking and if servicing was added.
            if (getBooking.BookedAt.AddDays(1) < DateTime.Now)
            {
                amendDetails.Charge = (int)(getBooking.Price / 10) * 100;
                var price = HttpContext.Session.GetObjectFromJson<int>("AmendCost");

                if (price > 0)
                    amendDetails.Charge += (price * 100);
            }
            else
            {
                var price = HttpContext.Session.GetObjectFromJson<int>("AmendCost");

                if (price > 0)
                    amendDetails.Charge = (price * 100);
            }

            //Gets booking from database.
            Booking b = _context.Bookings.Find(amendDetails.BookingId);

            //Checks if servicing was added.
            if (b.Servicing != amendDetails.Booking.Servicing && amendDetails.Booking.Servicing.Equals(true))
                amendDetails.Charge += 1500;

            //Sets up a Session equal to amendDetails.
            HttpContext.Session.SetObjectAsJson("AmendingFull", amendDetails);

            //Returns Checkout View with bookingDetails as model.
            return View(amendDetails);
        }

        /// <summary>
        /// Amends a booking and charges the customer if appropriate.
        /// </summary>
        /// <param name="stripeEmail">String Value</param>
        /// <param name="stripeToken">String Value</param>
        /// <returns>Redirect to UpdateBooking Action.</returns>
        [Authorize(Roles = "Booking Clerk,Admin,Manager,Customer")]
        [HttpPost, ActionName("AmendBooking")]
        public async Task<IActionResult> FinishAmending(string stripeEmail, string stripeToken)
        {
            //Gets class instance of AmendViewModel from Session.
            AmendViewModel amendDetails = HttpContext.Session.GetObjectFromJson<AmendViewModel>("AmendingFull");

            //Gets Claim class instance to identify current logged in user.
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //Checks if current slot is available.
            Slot slot = await _context.Slots.Where(s => s.Id == SlotAvailable(amendDetails.Booking.DepartureDate,
             amendDetails.Booking.ReturnDate, amendDetails.Booking.Id)).FirstOrDefaultAsync();

            //Returns view if no slot found.
            if (slot.Id == 0 || slot == null)
                return View();

            //Stripe Logic
            if (stripeToken != null)
            {
                //Initialises local variables.
                var customers = new CustomerService();
                var charges = new ChargeService();
                var customer = customers.Create(new CustomerCreateOptions
                {
                    Email = stripeEmail,
                    SourceToken = stripeToken
                });

                //Checks if it has been a day since booking was created.
                if (amendDetails.Booking.BookedAt.AddDays(1) < DateTime.Now)
                {
                    //Charges user appropriately.
                    var charge = charges.Create(new ChargeCreateOptions
                    {
                        Amount = Convert.ToInt32(amendDetails.Charge),
                        Description = "Booking Id: " + amendDetails.Booking.Id,
                        Currency = "gbp",
                        CustomerId = customer.Id
                    });

                    //Checks if charge was successful.
                    if (charge.Status.ToLower() == "succeeded")
                    {
                        //Sends invoice
                        await _emailSender.SendEmailAsync(_context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email,
                        "Parkeasy - Booking Amended Successful", "Your booking has been amended successfully, you are re-assigned to Slot " + slot.Id.ToString());

                        //Creates invoice for database.
                        Parkeasy.Models.Invoice invoiceNew = new Parkeasy.Models.Invoice
                        {
                            Price = amendDetails.Charge,
                            InvoiceBody = "Your booking has been amended successfully, you are re-assigned to Slot " + slot.Id.ToString(),
                            InvoiceType = "Booking Amended Successful",
                            Email = _context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email
                        };

                        //Adding invoice to database and creating Sessions.
                        slot.Bookings.Add(amendDetails.Booking);
                        HttpContext.Session.SetObjectAsJson("BookingAmend", amendDetails.Booking);
                        HttpContext.Session.SetObjectAsJson("FlightAmend", amendDetails.Flight);
                        HttpContext.Session.SetObjectAsJson("VehicleAmend", amendDetails.Vehicle);
                        HttpContext.Session.SetObjectAsJson("SlotAmend", slot);
                        _context.Invoices.Add(invoiceNew);

                        //Saving changes to database.
                        await _context.SaveChangesAsync();

                        //Redirects to UpdateBooking action.
                        return RedirectToAction(nameof(UpdateBooking));
                    }
                }
                if(amendDetails.Charge != 0)
                {
                //Creates new ChargeCreateOptions class instance.
                var newCharge = charges.Create(new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(amendDetails.Charge),
                    Description = "Booking Id: " + amendDetails.Booking.Id,
                    Currency = "gbp",
                    CustomerId = customer.Id
                });
                }

                //Sends invoice.
                await _emailSender.SendEmailAsync(_context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email,
                    "Parkeasy - Booking Amended Successful", "Your booking has been amended successfully, you are re-assigned to Slot " + slot.Id.ToString());

                //Creates invoice to add to database.
                Parkeasy.Models.Invoice newInvoice = new Parkeasy.Models.Invoice
                {
                    Price = 0,
                    InvoiceBody = "Your booking has been amended successfully, you are re-assigned to Slot " + slot.Id.ToString(),
                    InvoiceType = "Booking Amended Successful",
                    Email = _context.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email
                };

                //Creates sessions and adds invoice to database and saves changes to database.
                HttpContext.Session.SetObjectAsJson("BookingAmend", amendDetails.Booking);
                HttpContext.Session.SetObjectAsJson("FlightAmend", amendDetails.Flight);
                HttpContext.Session.SetObjectAsJson("VehicleAmend", amendDetails.Vehicle);
                HttpContext.Session.SetObjectAsJson("SlotAmendId", slot.Id);
                _context.Invoices.Add(newInvoice);
                await _context.SaveChangesAsync();

                //Redirect to UpdateBooking Action.
                return RedirectToAction(nameof(UpdateBooking));

            }
            return View();
        }

        /// <summary>
        /// Gets booking from database or Session if session is not null, then passes to AmendView.
        /// </summary>
        /// <param name="id">nullable integer value</param>
        /// <returns>Redirect to Amend View</returns>
        public IActionResult Amend(int? id)
        {
            Booking booking = _context.Bookings.Find(id);
            Booking amendBooking = HttpContext.Session.GetObjectFromJson<Booking>("BookingReAmend");
            if (amendBooking == null)
                amendBooking = booking;
            return View(amendBooking);
        }

        /// <summary>
        /// Gets appropriate info and passes them in sessions to other methods to update in database without conflicts.
        /// </summary>
        /// <param name="bookingAmend">Instance of Booking Class</param>
        /// <returns>return RedirectToAction AmendBooking with id param</returns>
        [HttpPost]
        public IActionResult Amend(Booking bookingAmend)
        {
            //Initialising local variables.
            //Getting id from parameter, getting booking using it and calculating price.
            int id = bookingAmend.Id;
            Booking booking = _context.Bookings.Find(id);
            
            if (bookingAmend.DepartureDate > DateTime.Now && bookingAmend.ReturnDate > bookingAmend.DepartureDate)
            {
            bookingAmend.Duration = (int) (bookingAmend.ReturnDate - bookingAmend.DepartureDate).Days;
            int price = (bookingAmend.Duration - booking.Duration) * (int)_context.Pricing.Last().PerDay;

            //Initialising Object Sessions and redirecting to AmendBooking action passing id.
            HttpContext.Session.SetObjectAsJson("AmendBooking", bookingAmend);
            HttpContext.Session.SetObjectAsJson("BookingReAmend", bookingAmend);
            HttpContext.Session.SetObjectAsJson("AmendCost", price);
            return RedirectToAction(nameof(AmendBooking), id);
            }
            else
                return View(bookingAmend);
        }

        /// <summary>
        /// Updates Booking info received from a session.
        /// </summary>
        /// <returns>Redirect to UpdateFlight action.</returns>
        public IActionResult UpdateBooking()
        {
            Booking booking = HttpContext.Session.GetObjectFromJson<Booking>("BookingAmend");
            _context.Update(booking);
            _context.SaveChanges();
            return RedirectToAction(nameof(UpdateFlight));
        }

        /// <summary>
        /// Updates Flight info received from a session.
        /// </summary>
        /// <returns>Redirect to UpdateVehicle action.</returns>
        public IActionResult UpdateFlight()
        {
            Flight flight = HttpContext.Session.GetObjectFromJson<Flight>("FlightAmend");
            _context.Update(flight);
            _context.SaveChanges();
            return RedirectToAction(nameof(UpdateVehicle));
        }

        /// <summary>
        /// Updates Vehicle info received from a session.
        /// </summary>
        /// <returns>Redirect to UpdateSlot action.</returns>
        public IActionResult UpdateVehicle()
        {
            Vehicle vehicle = HttpContext.Session.GetObjectFromJson<Vehicle>("VehicleAmend");
            _context.Update(vehicle);
            _context.SaveChanges();
            return RedirectToAction(nameof(UpdateSlot));
        }

        /// <summary>
        /// Updates slot info received from a session.
        /// </summary>
        /// <returns>Redirect to UserBookings action.</returns>
        public IActionResult UpdateSlot()
        {
            int slotId = HttpContext.Session.GetObjectFromJson<int>("SlotAmendId");
            Slot slot = _context.Slots.Find(slotId);
            Booking booking = HttpContext.Session.GetObjectFromJson<Booking>("BookingAmend");
            slot.Bookings.Add(booking);
            _context.Update(slot);
            _context.SaveChanges();
            return RedirectToAction(nameof(UserBookings));
        }

        #endregion

        #region reports
        /// <summary>
        /// Gets Appropriate BookingReport details from database and returns them in view for pdf.
        /// </summary>
        /// <returns>BookingReport View passing in List of BookingReport class instances.</returns>
        public async Task<IActionResult> BookingReport()
        {
            //Initialising local variables.
            int nowDay = DateTime.Now.Day;
            int nowMonth = DateTime.Now.Month;
            int nowYear = DateTime.Now.Year;

            IEnumerable<BookingReport> bReports = await _context.BookingReports.Where(
                br => br.ReportDay == nowDay && br.ReportMonth == nowMonth && br.ReportYear == nowYear
                ).ToListAsync();

            //Returns BookingReport view with param of list of BookingReport class instances.
            return View(bReports);
        }

        /// <summary>
        /// Gets Appropriate ReleseReports from database for PDF report.
        /// </summary>
        /// <returns>returns ReleaseReport view</returns>
        public async Task<IActionResult> ReleaseReport()
        {
            //Initialises local variables.
            int nowDay = DateTime.Now.Day;
            int nowMonth = DateTime.Now.Month;
            int nowYear = DateTime.Now.Year;

            //List of ReleaseReports with appropriate property values.
            IEnumerable<ReleaseReport> rReports = await _context.ReleaseReports.Where(
                br => br.ReportDay == nowDay && br.ReportMonth == nowMonth && br.ReportYear == nowYear
                ).ToListAsync();

            //Returns view with ReleaseReports.
            return View(rReports);
        }

        /// <summary>
        /// Gets Valeting Reports for appropriate date from database.
        /// </summary>
        /// <returns>Returns ValetingReport view</returns>
        public async Task<IActionResult> ValetingReport()
        {
            //Initialise local variables.
            int nowDay = DateTime.Now.Day;
            int nowMonth = DateTime.Now.Month;
            int nowYear = DateTime.Now.Year;

            //Gets appropriate Valeting Reports from database.
            IEnumerable<ValetingReport> vReports = await _context.ValetingReports.Where(
                br => br.ReportDay == nowDay && br.ReportMonth == nowMonth && br.ReportYear == nowYear
                ).ToListAsync();

            //Returns view passing in list of ValetingReport class instances.
            return View(vReports);
        }

        /// <summary>
        /// Gets appropriate TurnoverReport details for report.
        /// </summary>
        /// <returns>Returns TurnoverReport View passing in tReports</returns>
        public async Task<IActionResult> TurnoverReport()
        {
            //Gets all bookings and initialises local variables.
            var bookings = await _context.Bookings.ToListAsync();
            List<TurnoverReport> tReports = new List<TurnoverReport>();

            //Loops for all bookings.
            foreach (Booking booking in bookings)
            {
                //Does appropriate date checks and adds TurnoverReports to database where necessary.
                if (booking.BookedAt.Month == DateTime.Now.Month && booking.BookedAt.Year == DateTime.Now.Year)
                {
                    if (booking.Servicing.Equals(true))
                        booking.Price += _context.Pricing.Last().ServicingCost;

                    tReports.Add(new TurnoverReport
                    {
                        BookingId = booking.Id,
                        BookingDate = booking.BookedAt,
                        Price = booking.Price,
                        ReportMonth = DateTime.Now.Month,
                        ReportYear = DateTime.Now.Year
                    });
                }
            }

            //Returns view passing in TurnoverReport Class instance list.
            return View(tReports);
        }

        /// <summary>
        /// Generates view for MonhtlyBookingsReport Pdf.
        /// </summary>
        /// <returns>returns MonthlyBookingsReport View</returns>
        public async Task<IActionResult> MonthlyBookingsReport()
        {
            //Initialising local variables.
            int days = 31;
            var bookings = await _context.Bookings.ToListAsync();
            List<MonthlyBookingReport> bReports = new List<MonthlyBookingReport>();
            int[] noOfBookings = new int[10000] ;
            double[] total = new double[10000];

            //Sets days in a month appropriately.
            if (DateTime.Now.Month.Equals("February"))
                days = 29;
            else if (DateTime.Now.Month.Equals("November") || DateTime.Now.Month.Equals("September") || DateTime.Now.Month.Equals("June") || DateTime.Now.Month.Equals("April"))
                days = 30;

            //Loops for appropriate number of days.
            for (int i = 1; i <= days; i++)
            {
                //Loops all bookings.
                foreach (Booking booking in bookings)
                {
                    //does checks and changes noOfBookings and total appropriately.
                    if (booking.BookedAt.Month == DateTime.Now.Month && booking.BookedAt.Year == DateTime.Now.Year)
                    {
                        if (booking.BookedAt.Day == i)
                        {
                            noOfBookings[i]++;
                            total[i] += booking.Price;

                            if (booking.Servicing.Equals(true))
                            total[i] += _context.Pricing.Last().ServicingCost;
                        }
                    }
                }
                //Adds MonthlyBookingReport class instance to list.
                bReports.Add(new MonthlyBookingReport
                {
                    NoOfBookings = noOfBookings[i],
                    TotalAmount = total[i],
                    ReportDay = i,
                    ReportMonth = DateTime.Now.Month,
                    ReportYear = DateTime.Now.Year
                });
            }
            //Returns view passing in list of MonthlyBookingReport class instances.
            return View(bReports);
        }
        #endregion

        #region BuildReport
        /// <summary>
        /// Adds BookingReport to database.
        /// </summary>
        /// <returns>Redirect to ValetingStaffIndex action.</returns>
        public async Task<IActionResult> AddBookingReport()
        {
            //Gets id from session.
            int id = HttpContext.Session.GetObjectFromJson<int>("BookingId");

            //Gets appropriate booking and vehicle from database using id.
            Booking booking = _context.Bookings.Find(id);
            Vehicle vehicle = _context.Vehicles.Find(id);
            booking.ApplicationUser = _context.Users.Find(booking.ApplicationUserId);

            //Gets all BookingReports from database.
            List<BookingReport> bReports = await _context.BookingReports.ToListAsync();

            //Makes sure BookingReport does not already exist for booking.
            foreach (BookingReport brs in bReports)
            {
                if (brs.BookingId == id)
                    return RedirectToAction(nameof(ValetingStaffIndex));
            }

            //Renames firstname and lastname appropriately if blank.
            if (booking.ApplicationUser.FirstName == null && booking.ApplicationUser.LastName == null)
            {
                booking.ApplicationUser.FirstName = "External Login";
                booking.ApplicationUser.LastName = "User";
            }

            //Rewords PhoneNumber appropriately if blank.
            if (booking.ApplicationUser.PhoneNumber == null)
                booking.ApplicationUser.PhoneNumber = "Not Available";

            //Creates new BookingReport class instance.
            BookingReport bReport = new BookingReport
            {
                Name = booking.ApplicationUser.FirstName + " " + booking.ApplicationUser.LastName,
                ContactDetails = booking.ApplicationUser.PhoneNumber,
                Registration = vehicle.Registration,
                Model = vehicle.Model,
                ArrivalTime = DateTime.Now,
                DepartureTime = booking.DepartureDate,
                ReportDay = DateTime.Now.Day,
                ReportMonth = DateTime.Now.Month,
                ReportYear = DateTime.Now.Year,
                BookingId = booking.Id
            };

            //Adds BookingReport to database and saves.
            _context.BookingReports.Add(bReport);
            await _context.SaveChangesAsync();

            //Redirects to ValetingStaffIndex action.
            return RedirectToAction(nameof(ValetingStaffIndex));
        }

        /// <summary>
        /// Adds ReleaseReport to database.
        /// </summary>
        /// <returns>Redirect to ValetingStaffIndex action.</returns>
        public async Task<IActionResult> AddReleaseReport()
        {
            //Gets id from session.
            int id = HttpContext.Session.GetObjectFromJson<int>("ReleaseBookingId");

            //Loads appropraite booking and vehicle class instances from database using id.
            Booking booking = _context.Bookings.Find(id);
            Vehicle vehicle = _context.Vehicles.Find(id);

            //Gets Associated ApplicationUser for booking from database.
            booking.ApplicationUser = _context.Users.Find(booking.ApplicationUserId);

            //Gets list of all ReleaseReports.
            List<ReleaseReport> rReports = await _context.ReleaseReports.ToListAsync();

            //Loops all the ReleaseReports and makes sure that the report does not already exist.
            foreach (var rrs in rReports)
            {
                if (rrs.BookingId == id)
                    return RedirectToAction(nameof(ValetingStaffIndex));
            }

            //Sets firstname and lastname appropriately if blank.
            if (booking.ApplicationUser.FirstName == null && booking.ApplicationUser.LastName == null)
            {
                booking.ApplicationUser.FirstName = "External Login";
                booking.ApplicationUser.LastName = "User";
            }

            //Sets phonenumber appropriately if blank.
            if (booking.ApplicationUser.PhoneNumber == null)
                booking.ApplicationUser.PhoneNumber = "Not Available";

            //Creates ReleaseReport class instance populated.
            ReleaseReport rReport = new ReleaseReport
            {
                Name = booking.ApplicationUser.FirstName + " " + booking.ApplicationUser.LastName,
                ContactDetails = booking.ApplicationUser.PhoneNumber,
                Registration = vehicle.Registration,
                Model = vehicle.Model,
                ArrivalTime = booking.ReturnDate,
                ReportDay = DateTime.Now.Day,
                ReportMonth = DateTime.Now.Month,
                ReportYear = DateTime.Now.Year,
                BookingId = booking.Id
            };

            //Adds ReleaseReport class instance to database and saves.
            _context.ReleaseReports.Add(rReport);
            await _context.SaveChangesAsync();

            //Redirects to ValetingStaffIndex action.
            return RedirectToAction(nameof(ValetingStaffIndex));
        }

        /// <summary>
        /// Adds Valeting Report to database.
        /// </summary>
        /// <returns>redirect to ValetingStaffIndex Action</returns>
        public async Task<IActionResult> AddValetingReport()
        {
            //Gets id from session.
            int id = HttpContext.Session.GetObjectFromJson<int>("ValetBookingId");

            //Gets appropriate booking and vehicle details.
            Booking booking = _context.Bookings.Find(id);
            Vehicle vehicle = _context.Vehicles.Find(id);

            //Gets list of valetingreports to make sure report does not already exist.
            List<ValetingReport> vReports = await _context.ValetingReports.ToListAsync();

            //Loops each report and makes sure no report has a matching id to booking.
            foreach (var vrs in vReports)
            {
                if (vrs.BookingId == id)
                    return RedirectToAction(nameof(ValetingStaffIndex));
            }

            //Creates ValetingReport class instance.
            ValetingReport vReport = new ValetingReport
            {
                Registration = vehicle.Registration,
                Model = vehicle.Model,
                ReportDay = DateTime.Now.Day,
                ReportMonth = DateTime.Now.Month,
                ReportYear = DateTime.Now.Year,
                BookingId = booking.Id
            };

            //Adding class instance to database and saving it.
            _context.ValetingReports.Add(vReport);
            await _context.SaveChangesAsync();

            //Redirect to ValetingStaffIndex action.
            return RedirectToAction(nameof(ValetingStaffIndex));
        }

        #endregion

        /// <summary>
        /// Passes in a string that will run cases to generate the appropriate report.
        /// </summary>
        /// <param name="reportChoice">String variable.</param>
        /// <returns>PDF Report Download</returns>
        [Authorize(Roles = "Admin,Manager,Booking Clerk,Invoice Clerk,Driver,Valeting Staff")]
        public IActionResult CreatePDF(string reportChoice)
        {
            //Checks if passed variable is null, if so gets data from a backup session.
            if (reportChoice == null)
            {
                reportChoice = HttpContext.Session.GetObjectFromJson<string>("ReportChoice");
            }

            //Initialising local variable objectSettings to null.
            ObjectSettings objectSettings = null;

            //Setup local variable globalSettings.
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "PDF Report"
            };

            //Case statement that generates the objectSettings as appropriate to case.
            switch (reportChoice)
            {
                case "Monthly Turnover Report":
                    objectSettings = new ObjectSettings
                    {
                        PagesCount = true,
                        Page = "http://localhost:5000/Booking/TurnoverReport"
                    };
                    break;
                case "Booking Report":
                    objectSettings = new ObjectSettings
                    {
                        PagesCount = true,
                        Page = "http://localhost:5000/Booking/BookingReport"
                    };
                    break;
                case "Monthly Bookings Report":
                    objectSettings = new ObjectSettings
                    {
                        PagesCount = true,
                        Page = "http://localhost:5000/Booking/MonthlyBookingsReport"
                    };
                    break;
                case "Release Report":
                    objectSettings = new ObjectSettings
                    {
                        PagesCount = true,
                        Page = "http://localhost:5000/Booking/ReleaseReport"
                    };
                    break;
                case "Valeting Report":
                    objectSettings = new ObjectSettings
                    {
                        PagesCount = true,
                        Page = "http://localhost:5000/Booking/ValetingReport"
                    };
                    break;
                default:
                    return RedirectToAction(nameof(Reports));
            }

            //Initialises local variable pdf.
            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            //Uses _converter global to convert HtmltoPdfDocument.
            var file = _converter.Convert(pdf);

            //Returns the appropriate report download and name depending on case.
            switch (reportChoice)
            {
                case "Monthly Turnover Report":
                    return File(file, "application/pdf", "TurnoverReport.pdf");
                case "Booking Report":
                    return File(file, "application/pdf", "BookingReport.pdf");
                case "Monthly Bookings Report":
                    return File(file, "application/pdf", "MonthlyBookingReport.pdf");
                case "Release Report":
                    return File(file, "application/pdf", "ReleaseReport.pdf");
                case "Valeting Report":
                    return File(file, "application/pdf", "ValetingReport.pdf");
                default:
                    return RedirectToAction(nameof(Reports));
            }
        }
        /*public IActionResult GenerateReports()
        {
            try{
            if(_context.ReportDates.Where(rd => rd.ReportType.Equals("Booking Report")).Last().ReportDay == DateTime.Now.Day && _context.ReportDates.Where(rd => rd.ReportType.Equals("Booking Report")).Last().ReportMonth == DateTime.Now.Month && _context.ReportDates.Where(rd => rd.ReportType.Equals("Booking Report")).Last().ReportYear == DateTime.Now.Year)
            {}
            else
            {
                ReportDate rDate = new ReportDate{ReportType="Booking Report", ReportDay = DateTime.Now.Day, ReportMonth = DateTime.Now.Month, ReportYear = DateTime.Now.Year};
                _context.ReportDates.Add(rDate);
                _context.SaveChanges();
                
                HttpContext.Session.SetObjectAsJson("ReportChoice", "Booking Report");
                return RedirectToAction(nameof(CreatePDF));
            }
            } catch(Exception)
            {
                ReportDate rDate = new ReportDate{ReportType="Booking Report", ReportDay = DateTime.Now.Day, ReportMonth = DateTime.Now.Month, ReportYear = DateTime.Now.Year};
                _context.ReportDates.Add(rDate);
                _context.SaveChanges();
                
                HttpContext.Session.SetObjectAsJson("ReportChoice", new ReportViewModel{Report = "Booking Report"}.Report);
                return RedirectToAction(nameof(CreatePDF));
            }
            try{
            if(_context.ReportDates.Where(rd => rd.ReportType.Equals("Monthly Turnover Report")).Last().ReportDay == DateTime.Now.Day && _context.ReportDates.Where(rd => rd.ReportType.Equals("Booking Report")).Last().ReportMonth == DateTime.Now.Month && _context.ReportDates.Where(rd => rd.ReportType.Equals("Booking Report")).Last().ReportYear == DateTime.Now.Year)
            {}
            else
            {
                ReportDate rDate = new ReportDate{ReportType="Monthly Turnover Report", ReportDay = DateTime.Now.Day, ReportMonth = DateTime.Now.Month, ReportYear = DateTime.Now.Year};
                _context.ReportDates.Add(rDate);
                _context.SaveChanges();
                
                HttpContext.Session.SetObjectAsJson("ReportChoice", "Monthly Turnover Report");
                return RedirectToAction(nameof(CreatePDF));
            }
            } catch(Exception)
            {
                ReportDate rDate = new ReportDate{ReportType="Monthly Turnover Report", ReportDay = DateTime.Now.Day, ReportMonth = DateTime.Now.Month, ReportYear = DateTime.Now.Year};
                _context.ReportDates.Add(rDate);
                _context.SaveChanges();
                
                HttpContext.Session.SetObjectAsJson("ReportChoice", "Monthly Turnover Report");
                return RedirectToAction(nameof(CreatePDF));
            }
            try{
            if(_context.ReportDates.Where(rd => rd.ReportType.Equals("Release Report")).Last().ReportDay == DateTime.Now.Day && _context.ReportDates.Where(rd => rd.ReportType.Equals("Booking Report")).Last().ReportMonth == DateTime.Now.Month && _context.ReportDates.Where(rd => rd.ReportType.Equals("Booking Report")).Last().ReportYear == DateTime.Now.Year)
            {}
            else
            {
                 ReportDate rDate = new ReportDate{ReportType="Release Report", ReportDay = DateTime.Now.Day, ReportMonth = DateTime.Now.Month, ReportYear = DateTime.Now.Year};
                _context.ReportDates.Add(rDate);
                _context.SaveChanges();
                
                HttpContext.Session.SetObjectAsJson("ReportChoice", "Release Report");
                return RedirectToAction(nameof(CreatePDF));
            }
            } catch(Exception)
            {
                ReportDate rDate = new ReportDate{ReportType="Release Report", ReportDay = DateTime.Now.Day, ReportMonth = DateTime.Now.Month, ReportYear = DateTime.Now.Year};
                _context.ReportDates.Add(rDate);
                _context.SaveChanges();
                
                HttpContext.Session.SetObjectAsJson("ReportChoice", "Release Report");
                return RedirectToAction(nameof(CreatePDF));
            }
            try{
            if(_context.ReportDates.Where(rd => rd.ReportType.Equals("Monthly Bookings Report")).Last().ReportDay == DateTime.Now.Day && _context.ReportDates.Where(rd => rd.ReportType.Equals("Booking Report")).Last().ReportMonth == DateTime.Now.Month && _context.ReportDates.Where(rd => rd.ReportType.Equals("Booking Report")).Last().ReportYear == DateTime.Now.Year)
            {}
            else
            {
                 ReportDate rDate = new ReportDate{ReportType="Monthly Bookings Report", ReportDay = DateTime.Now.Day, ReportMonth = DateTime.Now.Month, ReportYear = DateTime.Now.Year};
                _context.ReportDates.Add(rDate);
                _context.SaveChanges();
                HttpContext.Session.SetObjectAsJson("ReportChoice", "Monthly Bookings Report");
                return RedirectToAction(nameof(CreatePDF));
            }
            } catch(Exception)
            {
                 ReportDate rDate = new ReportDate{ReportType="Monthly Bookings Report", ReportDay = DateTime.Now.Day, ReportMonth = DateTime.Now.Month, ReportYear = DateTime.Now.Year};
                _context.ReportDates.Add(rDate);
                _context.SaveChanges();
                HttpContext.Session.SetObjectAsJson("ReportChoice", "Monthly Bookings Report");
                return RedirectToAction(nameof(CreatePDF));
            }
            try{
            if(_context.ReportDates.Where(rd => rd.ReportType.Equals("Valeting Report")).Last().ReportDay == DateTime.Now.Day && _context.ReportDates.Where(rd => rd.ReportType.Equals("Booking Report")).Last().ReportMonth == DateTime.Now.Month && _context.ReportDates.Where(rd => rd.ReportType.Equals("Booking Report")).Last().ReportYear == DateTime.Now.Year)
            {}
            else
            {
                 ReportDate rDate = new ReportDate{ReportType="Valeting Report", ReportDay = DateTime.Now.Day, ReportMonth = DateTime.Now.Month, ReportYear = DateTime.Now.Year};
                _context.ReportDates.Add(rDate);
                _context.SaveChanges();
                HttpContext.Session.SetObjectAsJson("ReportChoice", "Valeting Report");
                return RedirectToAction(nameof(CreatePDF));
            }
            } catch(Exception)
            {
                 ReportDate rDate = new ReportDate{ReportType="Valeting Report", ReportDay = DateTime.Now.Day, ReportMonth = DateTime.Now.Month, ReportYear = DateTime.Now.Year};
                _context.ReportDates.Add(rDate);
                _context.SaveChanges();
                HttpContext.Session.SetObjectAsJson("ReportChoice", "Valeting Report");
                return RedirectToAction(nameof(CreatePDF));
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }*/

        /// <summary>
        /// Makes selectlist and returns view.
        /// </summary>
        /// <returns>AddPickup View</returns>
        [Authorize(Roles = "Admin,Manager,Booking Clerk")]
        public IActionResult AddPickup()
        {
            //List of Pickup class instances for selectlist.
            List<Pickup> locations = new List<Pickup>();

            //Add to List of Pickup class instances.
            locations.Add(new Pickup { Location = "Car Park" });
            locations.Add(new Pickup { Location = "Airport" });

            //Creates ViewData "Locations" for displaying Location dropdown in View.
            ViewData["Location"] = new SelectList(locations, "Location", "Location");

            //Returns AddPickup view.
            return View();
        }

        /// <summary>
        /// Adds pickup date and location to database.
        /// </summary>
        /// <param name="model">Instance of Pickup Class.</param>
        /// <returns>Redirect to Homepage or AddPickup View</returns>
        [HttpPost]
        public async Task<IActionResult> AddPickup(Pickup model)
        {
            try
            {
                if(model.PickupDate > DateTime.Now)
                {
                //Updates model, adds it to database and saves changes.
                model.Status = "NotArrived";
                _context.Pickups.Add(model);
                await _context.SaveChangesAsync();

                //Redirects to homepage.
                return RedirectToAction(nameof(HomeController.Home), "Home");
                }
                else
                {
                    return View(model);
                }
            }
            catch(Exception)
            {
                //Returns AddPickup View.
                return View(model);
            }
        }

        /// <summary>
        /// Changes status of pickup class instance.
        /// </summary>
        /// <param name="id">Nullable integer value.</param>
        /// <returns>Redirect to DriverIndex action</returns>
        public async Task<IActionResult> ConfirmPickup(int? id)
        {
            Pickup pickup = _context.Pickups.Find(id);

            pickup.Status = "Arrived";
            _context.Pickups.Update(pickup);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(DriverIndex));
        }

        /// <summary>
        /// Changes status of pickup class instance.
        /// </summary>
        /// <param name="id">Nullable integer value.</param>
        /// <returns>Redirect to DriverIndex action</returns>
        public async Task<IActionResult> ConfirmDropOff(int? id)
        {
            Pickup pickup = _context.Pickups.Find(id);

            pickup.Status = "Complete";
            _context.Pickups.Update(pickup);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(DriverIndex));
        }

        /// <summary>
        /// Gets list of pickups due that day and display them on a view with options.
        /// </summary>
        /// <returns>DriverIndex View.</returns>
        public IActionResult DriverIndex()
        {
            //Initialising local variable.
            int day = DateTime.Now.Day;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            //List of pickups due on that date.
            IEnumerable<Pickup> pickups = _context.Pickups.Where(p => p.PickupDate.Day == day && p.PickupDate.Month == month && p.PickupDate.Year == year);

            //Returns DriverIndex View.
            return View(pickups);
        }
    }
}