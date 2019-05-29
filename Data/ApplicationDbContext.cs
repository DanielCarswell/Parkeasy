using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parkeasy.Models;
using Parkeasy.Models.Reports;

namespace Parkeasy.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        /// <summary>
        /// Initialises Bookings Table in Database.
        /// </summary>
        public DbSet<Booking> Bookings { get; set; }
        /// <summary>
        /// Initialises Vehicles Table in Database.
        /// </summary>
        public DbSet<Vehicle> Vehicles{ get; set; }
        /// <summary>
        /// Initialises Flights Table in Database.
        /// </summary>
        public DbSet<Flight> Flights { get; set; }
        /// <summary>
        /// Initialises Invoices Table in Database.
        /// </summary>
        public DbSet<Invoice> Invoices{ get; set; }
        /// <summary>
        /// Initialises Slots Table in Database.
        /// </summary>
        public DbSet<Slot> Slots{ get; set; }
        /// <summary>
        /// Initialises Pricing Table in Database.
        /// </summary>
        public DbSet<Pricing> Pricing { get; set; }
        /// <summary>
        /// Initialises BookingReport Table in Database.
        /// </summary>
        public DbSet<BookingReport> BookingReports { get; set; }
        /// <summary>
        /// Initialises ReleaseReport Table in Database.
        /// </summary>
        public DbSet<ReleaseReport> ReleaseReports { get; set; }
        /// <summary>
        /// Initialises ValetingReport Table in Database.
        /// </summary>
        public DbSet<ValetingReport> ValetingReports { get; set; }
        /// <summary>
        /// Initialises ReportDates Table in Database.
        /// </summary>
        public DbSet<ReportDate> ReportDates { get; set; }
    }
}
